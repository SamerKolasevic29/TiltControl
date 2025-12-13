
namespace TiltControl.Models
{
//  derived Class Obstacle for having independent behavior
    public class Obstacle : GameObject 
    {
        public override Color DiscColor { get; set; }

        public Obstacle(double _x_, double _y_, double _objectSize_, Color _c_) : base(_x_, _y_, _objectSize_)
        {
            DiscColor = _c_;
        }

        
    }
}
