using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GithubSync
{
    public class GeometryAnimation : GeometryAnimationBase
    {
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register("Geometry", typeof(PathGeometry), typeof(GeometryAnimation));

        public override Geometry GetCurrentValueCore(Geometry defaultOriginValue, Geometry defaultDestinationValue, AnimationClock animationClock)
        {
            var progress = animationClock.CurrentProgress.Value;

            foreach (var f in Geometry.Figures)
            {
                foreach (var s in f.Segments)
                {

                }
            }

            return null;
        }

        public PathGeometry Geometry
        {
            get
            {
                return (PathGeometry)GetValue(GeometryProperty);
            }
            set
            {
                SetValue(GeometryProperty, value);
            }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new GeometryAnimation();
        }

        public override bool IsDestinationDefault
        {
            get
            {
                return true;
            }
        }
    }
}
