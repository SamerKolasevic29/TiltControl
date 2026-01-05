using TiltControl.Enums;


namespace TiltControl.Logic
{
    public static class ControllerMapper
    {
        public static(AccState acc, StrState str) MapReadings(float readingX, float readingY )
        {
            return (GetAccState(-readingX), GetStrState(readingY));
        }

        public static AccState GetAccState(float val)
        {
            float abs = Math.Abs(val);
            

            bool isFwd = val > 0;

            return abs switch
            {
                <= 0.20f => AccState.STOP,
                <= 0.44f => isFwd ? AccState.FWD1 : AccState.BCK1,
                <= 0.64f => isFwd ? AccState.FWD2 : AccState.BCK2,
                _ => isFwd ? AccState.FWD3 : AccState.BCK3
            };
        }

        public static StrState GetStrState(float val) 
        {
            float abs = Math.Abs(val);


            bool isRgt = val > 0;

            return abs switch
            {
                <= 0.20f => StrState.STOP,
                <= 0.44f => isRgt ? StrState.RGT1 : StrState.LFT1,
                <= 0.64f => isRgt ? StrState.RGT2 : StrState.LFT2,
                _ => isRgt ? StrState.RGT3 : StrState.LFT3
            };
        }

       
    }
}
