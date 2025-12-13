
namespace TiltControl.Models
{

//  derived Class Disc for havign independent behavior
    public class Disc : GameObject
    {
        public override Color DiscColor { get; set; }
        public override double SpeedFactor { get; } = 0.04;
        public override double PredictedX { get; set; }
        public override double PredictedY { get; set; }

        public Disc(double _x_, double _y_, double _objectSize_, Color _c_) : base(_x_, _y_, _objectSize_) {
            DiscColor = _c_;
            PredictedX = _x_;
            PredictedY = _y_;
            }


        //      calculating predicted position based on current reading without committing the move
        public void MakePrediction(float readingX, float readingY)
        {
            double deltaX = -readingX * SpeedFactor;
            double deltaY = +readingY * SpeedFactor;

            PredictedX = Math.Clamp(X + deltaX, 0, 1);
            PredictedY = Math.Clamp(Y + deltaY, 0, 1);
        }


//      applying predicted position as actual position when collision detected (no movement)
        public void Effect()
        {
            PredictedX = X;
            PredictedY = Y;
        }

//      committing the move when no collision detected
        public void CommitMove()
        {    
            X = PredictedX;
            Y = PredictedY;
        }
//      static method to check collision between Disc and Obstacle
        public static bool CheckCollision(Disc d, Obstacle o, double layoutWidth, double layoutHeight)
        {
//          Convert predicted normalized coordinates to DIU coordinates
            double predictedXInDIU = (d.PredictedX * (layoutWidth - d.ObjectSize)) + d.ObjectRadius;
            double predictedYInDIU = (d.PredictedY * (layoutHeight - d.ObjectSize)) + d.ObjectRadius;

//          Calculate squared distance between centers
            double dxSquared = (predictedXInDIU - o.XInDUI) * (predictedXInDIU - o.XInDUI);
            double dySquared = (predictedYInDIU - o.YInDUI) * (predictedYInDIU - o.YInDUI);

//          Calculate squared sum of radiuss
            double radiusSumSquared = (d.ObjectRadius + o.ObjectRadius) * (d.ObjectRadius + o.ObjectRadius);

//          Check for collision
            return (dxSquared + dySquared) < radiusSumSquared;
        }
    }
}
