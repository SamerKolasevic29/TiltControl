
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


        // calculating predicted position based on current reading without committing the move
        public void MakePrediction(float readingX, float readingY)
        {
            double deltaX = -readingX * SpeedFactor;
            double deltaY = +readingY * SpeedFactor;

            PredictedX = Math.Clamp(X + deltaX, 0, 1);
            PredictedY = Math.Clamp(Y + deltaY, 0, 1);
        }


//      committing the move when no collision detected
        public void CommitMove()
        {    
            X = PredictedX;
            Y = PredictedY;
        }
        // Collision detection and resolution with an obstacle
        public void ResolveCollision(Obstacle o, double layoutWidth, double layoutHeight)
        {
            // 1. Convert PredictedX/Y from 0-1 to DIU coordinates
            double predictedXInDIU = (PredictedX * (layoutWidth - ObjectSize)) + ObjectRadius;
            double predictedYInDIU = (PredictedY * (layoutHeight - ObjectSize)) + ObjectRadius;

            // 2. Calculate vector from obstacle center to predicted position
            double vecX = predictedXInDIU - o.XInDUI;
            double vecY = predictedYInDIU - o.YInDUI;

            // 3. Calculate distance between centers
            double distance = Math.Sqrt((vecX * vecX) + (vecY * vecY));

            // 4. Calculate minimum distance to avoid collision
            double minDistance = ObjectRadius + o.ObjectRadius;

            // 5. if distance is less than minimum distance, we have a collision
            if (distance < minDistance)
            {
                // Prevent division by zero
                // If the distance is zero, we can arbitrarily set vecX to 1
                if (distance == 0) { vecX = 1; distance = 1; }

                // Normalize the vector from obstacle center to predicted position
                double nX = vecX / distance;
                double nY = vecY / distance;

                // new position is: obstacle center + normalized vector * minDistance
                // this places the disc just outside the obstacle
                double newXInDIU = o.XInDUI + (nX * minDistance);
                double newYInDIU = o.YInDUI + (nY * minDistance);

                // Convert back to 0-1 range for PredictedX/Y
                // PredictedX/Y = (newPositionInDIU - ObjectRadius) / (layoutSize - ObjectSize)
                PredictedX = (newXInDIU - ObjectRadius) / (layoutWidth - ObjectSize);
                PredictedY = (newYInDIU - ObjectRadius) / (layoutHeight - ObjectSize);
            }
        }
    }
}
