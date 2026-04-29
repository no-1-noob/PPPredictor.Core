using System.ComponentModel;
namespace PPPredictor.Core.DataType.MapPool
{
    public class PPPWeightingInfo
    {
        [DefaultValue(0)]
        public double XParameter
        {
            get => _xParameter;
        }
        [DefaultValue(0)]
        public double YParameter
        {
            get => _yParameter;
        }
        [DefaultValue(0)]
        public double ZParameter
        {
            get => _zParameter;
        }
        [DefaultValue(0)]
        public float AccumulationConstant
        {
            get => _accumulationConstant;
        }
        
        private double _xParameter = 0;
        private double _yParameter = 0;
        private double _zParameter = 0;
        private float _accumulationConstant = 0;
        
        public PPPWeightingInfo(double xParameter, double yParameter, double zParameter)
        {
            _xParameter = xParameter;
            _yParameter = yParameter;
            _zParameter = zParameter;
        }
        public PPPWeightingInfo(float accumulationConstant)
        {
            _accumulationConstant = accumulationConstant;
        }

        public PPPWeightingInfo()
        {
            
        }
    }
}
