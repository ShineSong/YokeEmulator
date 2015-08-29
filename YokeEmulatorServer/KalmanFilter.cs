using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumberType = System.Double;

namespace YokeEmulatorServer
{
    /// <summary>
    /// Represents a class that can test out an implementation of the Kalman filter from 
    /// http://kkjkok.blogspot.com/2012/02/bayesian-approach-to-kalman-filtering.html
    /// </summary>
    public class KalmanFilter
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Search for "but where on earth did the pred_mean and pred_sigma functions come
        /// from" in the blog article for an explanation of this method.</summary>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public NumberType PredictedMean(NumberType sourceScale, NumberType previousMean)
        {
            return sourceScale * previousMean;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Search for "but where on earth did the pred_mean and pred_sigma functions come
        /// from" in the blog article for an explanation of this method.</summary>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public NumberType PredictedSigma(NumberType sourceScale, NumberType previousSigma, NumberType sourceSigma)
        {
            return (NumberType)Math.Sqrt((sourceScale * sourceScale) * (previousSigma * previousSigma) + (sourceSigma * sourceSigma));
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Explained at the beginning of the blog article</summary>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public NumberType UpdateMean(NumberType predictedMean, NumberType predictedSigma, NumberType measuredValue, NumberType measuredSigma)
        {
            NumberType numerator = (predictedMean / (predictedSigma * predictedSigma)) + (measuredValue / (measuredSigma * measuredSigma));

            NumberType denominator = ((NumberType)1 / (predictedSigma * predictedSigma)) + ((NumberType)1 / (measuredSigma * measuredSigma));

            return numerator / denominator;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Explained at the beginning of the blog article</summary>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public NumberType UpdateSigma(NumberType predictedSigma, NumberType measuredSigma)
        {
            double r = (1 / (predictedSigma * predictedSigma)) + (1 / (measuredSigma * measuredSigma));

            return (NumberType)(1.0 / Math.Sqrt(r));
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>Apply the kalman filter to our measured signal to produce the "filtered"
        /// signal</summary>
        /// <param name="measuredValues">The "measured" signal</param>
        /// <param name="sourceScale"></param>
        /// <param name="sourceSigma"></param>
        /// <param name="measuredSigma"></param>
        /// <returns></returns>
        ///////////////////////////////////////////////////////////////////////////////////////////
        public NumberType[] Filter(NumberType[] measuredValues, NumberType sourceScale, NumberType sourceSigma, NumberType measuredSigma)
        {
            NumberType lastMean = 0;

            NumberType lastSigma = sourceSigma;

            NumberType[] filteredValues = new NumberType[measuredValues.Length];

            for (int curEntryIndex = 0; curEntryIndex < measuredValues.Length; ++curEntryIndex)
            {
                NumberType estimatedMean = PredictedMean(sourceScale, lastMean);
                NumberType estimatedSigma = PredictedSigma(sourceScale, lastSigma, sourceSigma);

                filteredValues[curEntryIndex] = estimatedMean + (NumberType)SimpleRNG.GetNormal(0D, (double)estimatedSigma);

                lastMean = UpdateMean(estimatedMean, estimatedSigma, measuredValues[curEntryIndex], measuredSigma);
                lastSigma = UpdateSigma(estimatedSigma, measuredSigma);
            }

            return filteredValues;
        }

        public KalmanFilter(double _sourceSigma, double _measuredSigma)
        {
            lastMean = 0;
            sourceSigma = _sourceSigma;
            lastSigma = _sourceSigma;
            measuredSigma = _measuredSigma;
            sourceScale = (NumberType)Math.Sqrt(1 - (sourceSigma * sourceSigma));
        }
        
        NumberType lastMean;
        NumberType sourceSigma;
        NumberType lastSigma;
        NumberType sourceScale;
        NumberType measuredSigma;
        public NumberType Filter(NumberType i)
        {
            NumberType estimatedMean = PredictedMean(sourceScale, lastMean);
            NumberType estimatedSigma = PredictedSigma(sourceScale, lastSigma, sourceSigma);

            NumberType kf = estimatedMean + (NumberType)SimpleRNG.GetNormal(0D, (double)estimatedSigma);
            lastMean = UpdateMean(estimatedMean, estimatedSigma, i, measuredSigma);
            lastSigma = UpdateSigma(estimatedSigma, measuredSigma);
            return kf;
        }
    }
}
