using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace dohnadohna
{
    public static class StorageService
    {
        private const string StoreDirectory = @"AliceSoft\多娜多娜\SaveData\User";
        public static readonly string TargetDirectory;

        static StorageService()
        {
            TargetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), StoreDirectory);
            Directory.CreateDirectory(TargetDirectory);
        }

        public static void Save(DataModel data)
        {
            var path = Path.Combine(TargetDirectory, $"{data.Name}{Path.GetExtension(data.Image)}");
            if (data.Image != path)
                File.Copy(data.Image, path, true);
            data.Image = Path.GetFileName(path);

            var type = data.GetType();
            var properties = type.GetProperties();
            var filename = Path.Combine(TargetDirectory, $"{data.Name}.txt");
            using (var file = File.OpenWrite(filename))
            using (var writer = new StreamWriter(file, Encoding.Default))
            {
                foreach (var property in properties)
                {
                    var attr = property.GetCustomAttributes(typeof(IniNameAttribute), false).FirstOrDefault() as IniNameAttribute;
                    if (attr == null)
                        continue;

                    if (property.PropertyType.IsArray || property.PropertyType.GetInterface(nameof(System.Collections.ICollection)) != null)
                    {
                        dynamic items = property.GetValue(data);
                        foreach (var item in items)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            writer.WriteLine($"{attr.Name}={item}");
                        }
                    }
                    else 
                    {
                        writer.WriteLine($"{attr.Name}={property.GetValue(data)}");
                    }
                }
                writer.Flush();
                writer.Close();
            }

            data.Image = path;
        }

        public static DataModel Load(string filename)
        {
            Dictionary<string, List<string>> temp = new Dictionary<string, List<string>>();
            using (var file = File.OpenRead(filename))
            using (var reader = new StreamReader(file, Encoding.Default))
            {
                string line;
                do
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    var slice = line.Split('=');
                    var key = slice[0].Trim();
                    if (!temp.ContainsKey(key))
                    {
                        temp[key] = new List<string>();
                    }
                    temp[key].Add(slice[1].Trim());
                } while (line != null);
                reader.Close();
            }

            DataModel data;
            if (temp.ContainsKey(CustomerModel.CustomerUniqueName))
            {
                data = new CustomerModel();
            }
            else
            {
                data = new StaffModel();
            }

            var properties = data.GetType().GetProperties();
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttributes(typeof(IniNameAttribute), false).FirstOrDefault() as IniNameAttribute;
                if (attr == null)
                {
                    continue;
                }

                if (property.PropertyType.IsArray)
                {
                    property.SetValue(data, temp[attr.Name].ToArray());
                }
                else if (property.PropertyType.GetInterface(nameof(System.Collections.IList)) != null)
                {
                    property.SetValue(data, temp[attr.Name]);
                }
                else
                {
                    property.SetValue(data, temp[attr.Name].First()); 
                }
            }

            data.Image = Path.Combine(TargetDirectory, data.Image);
            return data;
        }
    }
}
