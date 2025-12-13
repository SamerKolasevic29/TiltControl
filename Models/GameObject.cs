
//escaping using this because is to repetative
namespace TiltControl.Models
{
    public abstract class GameObject
    {
        public double X { get; protected set; }  //ex-CurrentX
        public double Y { get; protected set; }  //ex-CurrentY
        public double ObjectSize { get; set; }   //ex-DiscSize
        public double ObjectRadius { get; private set; }

        //Device Independent Units (DIU) coordinates for rendering
        public double XInDUI {get; set; }   
        public double YInDUI { get; set; }

        //abstract property for derived Class Disc and Obstacle
        public abstract Color DiscColor { get; set; }
        public virtual double SpeedFactor { get; } 
        public virtual double PredictedX { get; set; }
        public virtual double PredictedY { get; set; }

        public GameObject(double _x_, double _y_, double _objectSize_)
        {
            X = Math.Clamp(_x_, 0, 1);
            Y = Math.Clamp(_y_, 0, 1);
            ObjectSize = _objectSize_;
            ObjectRadius = _objectSize_ / 2;

        }


        //      recalculating DIU coordinates based on current proportional position and layout size
        public void RecalculateDUICenter(double layoutWidth, double layoutHeight)
        {
            XInDUI = (X * (layoutWidth - ObjectSize)) + ObjectRadius;
            YInDUI = (Y * (layoutHeight - ObjectSize)) + ObjectRadius;
        }


    }
}
