﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CNTK
{
    public static class CNTKExtension
    {
        public static Function Dense(Variable operand, int outputDim, DeviceDescriptor device)
        {
            if (operand.Shape.Rank == 1)
                return FullyConnectedLinearLayer(operand, outputDim, device);

            //flatten input layer
            var newDim = operand.Shape.Dimensions.Aggregate((d1, d2) => d1 * d2);
            operand = CNTKLib.Reshape(operand, new int[] { newDim });

            return FullyConnectedLinearLayer(operand, outputDim, device);
        }

        public static Function Dense(Variable operand, int outputDim, DeviceDescriptor device, string name)
        {
            //flatten input layer if necessary
            if (operand.Shape.Rank == 1)
                return FullyConnectedLinearLayer(operand, outputDim, device, name);

            var newDim = operand.Shape.Dimensions.Aggregate((d1, d2) => d1 * d2);
            operand = CNTKLib.Reshape(operand, new[] { newDim });

            return FullyConnectedLinearLayer(operand, outputDim, device, name);
        }

        private static Function FullyConnectedLinearLayer(Variable input, int outputDim, DeviceDescriptor device,
            string name = "")
        {
            System.Diagnostics.Debug.Assert(input.Shape.Rank == 1);

            var inputDim = input.Shape[0];

            int[] weightMatrixDimensions = { outputDim, inputDim };
            var weights = new Parameter(weightMatrixDimensions, DataType.Float,
                CNTKLib.GlorotUniformInitializer(
                    CNTKLib.DefaultParamInitScale,
                    CNTKLib.SentinelValueForInferParamInitRank,
                    CNTKLib.SentinelValueForInferParamInitRank, 1),
                device, "weights");
            var timesFunction = CNTKLib.Times(weights, input, "times");

            int[] biasDimension = { outputDim };

            var bias = new Parameter(biasDimension, 0.0f, device, "plusParam");
            return CNTKLib.Plus(bias, timesFunction, name);
        }

        public static bool ReachedEndOfEpoch(this UnorderedMapStreamInformationMinibatchData minibatchData)
        {
            return minibatchData.Values.Any(stream => stream.sweepEnd);
        }
    }
}
