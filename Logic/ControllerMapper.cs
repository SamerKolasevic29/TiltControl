namespace TiltControl.Logic
{

    /* Discrete control levels derived from continuous sensor input.
     * STOP represents neutral position, FIVE is the maximum intensity. */
    public enum ControlLevel { STOP = 0, ONE = 1, TWO = 2, THREE = 3, FOUR = 4, FIVE = 5 }


    /* Maps raw accelerometer input into stable, discrete control levels
     *   using hysteresis to suppress noise and rapid oscillations.   
     *   This class is stateful by design: it remembers the last output level
     *   in order to apply entry/exit thresholds correctly.             */
    public class HysteresisMapper
    {
        //Last accepted (fwd/bck, lft/rgt) Control Levels
        private ControlLevel _lastAccLevel = ControlLevel.STOP;
        private ControlLevel _LastStrLevel = ControlLevel.STOP;

        // Thresholds for entering a higher control level (responsive ramp-up)
        private readonly float[] _entryThresholds = { 0.20f, 0.32f, 0.46f, 0.60f, 0.74f };

        // Thresholds for exiting the current control level (stability ramp-down)
        private readonly float[] _exitThreholds = { 0.15f, 0.28f, 0.42f, 0.56f, 0.70f };



        /* Converts raw acceleration values into a protocol string and
         * discrete control states for both axes.
         * returns Protocol string + resolved control levels and directions  */
        public (string protocol, ControlLevel accLvl, bool isFwd, ControlLevel strLvl, bool isRgt)
         GetState(float accRaw, float strRaw)
        {

            // Resolve acceleration and steering axis
            var (newAccLevel, isFwd) = CalculateLevel(accRaw, _lastAccLevel);
            var (newStrLevel, isRgt) = CalculateLevel(strRaw, _LastStrLevel);

            // Persist state for hysteresis processing
            _lastAccLevel = newAccLevel;
            _LastStrLevel = newStrLevel;

            // Building compact protocol representation
            string p1 = FormatPart(newAccLevel, isFwd, "FWD", "BCK");
            string p2 = FormatPart(newStrLevel, isRgt, "RGT", "LFT");

            return ($"{p1}{p2}", newAccLevel, isFwd, newStrLevel, isRgt);
        }

        /* Calculates a stable discrete control level from a raw sensor value,
         * applying hysteresis to prevent flickering around thresholds.
         * returns New control level and its direction                                          */
        private (ControlLevel, bool) CalculateLevel(float value, ControlLevel currentLevel)
        {
            float absValue = Math.Abs(value);
            bool isPositive = value > 0;

            // Determine the target level based purely on magnitude
            int targetInt = 0;
            for (int i = 0; i < 5; i++)
            {
                if (absValue >= _entryThresholds[i])
                {
                    targetInt = i + 1;
                }
            }

            // Hysteresis logic:
            // - Ramp up immediately for responsive control
            if (targetInt > (int)currentLevel)
            {
                return ((ControlLevel)targetInt, isPositive);
            }

            // - Ramp down only after crossing the exit threshol
            if (targetInt < (int)currentLevel)
            {
                int currentIdx = (int)currentLevel - 1;

                // Confirm drop only when sufficiently below exit threshold
                if (absValue < _exitThreholds[currentIdx])
                {
                    return ((ControlLevel)targetInt, isPositive);
                }
            }

            // Otherwise, remain at the current level
            return (currentLevel, isPositive);
        }


        /* Formats a protocol segment representing one axis.
         * returns Protocol Fragment                                            */ 
        private string FormatPart(ControlLevel level, bool isPos, string posTag, string negTag )
        {
            if (level == ControlLevel.STOP) return "STOP";
            return $"{(isPos ? posTag : negTag)}{(int)level}";       
        }
    }
}