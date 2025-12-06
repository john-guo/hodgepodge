import sys
import struct
import os

MAGIC = b'_@!M**B!@_' 

def embed(image_path, file_path, output_path):
    filename = os.path.basename(file_path).encode('utf-8')
    with open(image_path, 'rb') as img, open(file_path, 'rb') as f, open(output_path, 'wb') as out:
        img_data = img.read()
        file_data = f.read()
        out.write(img_data)
        out.write(file_data)
        out.write(filename)
        out.write(struct.pack('<H', len(filename))) 
        out.write(MAGIC)
        out.write(struct.pack('<Q', len(file_data))) 
    print(f"Ok")

def extract(image_with_file, output_dir):
    namecap = struct.calcsize('<H')
    sizecap = struct.calcsize('<Q')
    with open(image_with_file, 'rb') as f:
        f.seek(-len(MAGIC)-sizecap, os.SEEK_END) 
        magic = f.read(len(MAGIC))
        if magic != MAGIC:
            print("not vaild")
            return
        size = struct.unpack('<Q', f.read(sizecap))[0]

        f.seek(-(len(MAGIC)+namecap+sizecap), os.SEEK_END)
        name_len = struct.unpack('<H', f.read(namecap))[0]
        f.seek(-(len(MAGIC)+namecap+sizecap+name_len), os.SEEK_END)
        filename = f.read(name_len).decode('utf-8')

        f.seek(-(len(MAGIC)+namecap+sizecap+name_len+size), os.SEEK_END)
        file_data = f.read(size)

    output_path = os.path.join(output_dir, filename)
    with open(output_path, 'wb') as out:
        out.write(file_data)
    print(f"Extract {output_path}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage:")
        print("  Embed: python magicbox.py embed image.jpg file.txt output.jpg")
        print("  Extract: python magicbox.py extract image_with_file.jpg output_dir")
        sys.exit(1)

    mode = sys.argv[1]
    if mode == "embed":
        embed(sys.argv[2], sys.argv[3], sys.argv[4])
    elif mode == "extract":
        extract(sys.argv[2], sys.argv[3] if len(sys.argv) > 3 else os.getcwd())
    else:
        print("Argument wrong")