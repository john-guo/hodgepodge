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
    public abstract class GeometryAnimationBase : AnimationTimeline
    {
        public override Type TargetPropertyType
        {
            get
            {
                return typeof(Geometry);
            }
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (animationClock == null)
            {
                throw new ArgumentNullException("animationClock");
            }
            if (animationClock.CurrentState == ClockState.Stopped)
            {
                return defaultDestinationValue;
            }
            return GetCurrentValueCore((Geometry)defaultOriginValue, (Geometry)defaultDestinationValue, animationClock);
        }

        public abstract Geometry GetCurrentValueCore(Geometry defaultOriginValue, Geometry defaultDestinationValue, AnimationClock animationClock);
    }
}
