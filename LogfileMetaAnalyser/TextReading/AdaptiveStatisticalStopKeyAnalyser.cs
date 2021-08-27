using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser
{
    public class AdaptiveStatisticalStopKeyAnalyser
    {
        private int checkSample; //every n message a check is done
        private Regex rx_stopTerm;
        private int sampleInterNr = 0;
        private bool forceCheck = false;
        private int recalcSampleCounter = 0;
        private float statisticalWindowData = 0;
        private int statisticalWindowWidth = 0;
        private bool isWindowFull = false;
        private float statisticalWindowWidthMinusOne;
        private float statisticalWindowWidthRecip;
        private float statisticalWindowWidthMinusOneRel;

        public AdaptiveStatisticalStopKeyAnalyser(string stopTermRegExString)
        {
            rx_stopTerm = new Regex(stopTermRegExString, RegexOptions.Compiled | RegexOptions.Singleline);
            checkSample = 1; //set initially the number of samples to 1 == measure all incoming texts

            statisticalWindowWidthMinusOne = Constants.AdaptiveStatisticalStopKeyAnalyser_statisticalWindowWidth - 1;
            statisticalWindowWidthRecip = 1f / Constants.AdaptiveStatisticalStopKeyAnalyser_statisticalWindowWidth;
            statisticalWindowWidthMinusOneRel = statisticalWindowWidthMinusOne / Constants.AdaptiveStatisticalStopKeyAnalyser_statisticalWindowWidth;
        }

        public AdaptiveStatisticalStopKeyAnalyserResult CheckMsg(TextMessage msg)
        {
            if (!Constants.AdaptiveStatisticalStopKeyAnalyser_isEnabled)
                return AdaptiveStatisticalStopKeyAnalyserResult.Untested;

            AdaptiveStatisticalStopKeyAnalyserResult res = AdaptiveStatisticalStopKeyAnalyserResult.Unevolved;

            sampleInterNr++;
            recalcSampleCounter++;

            if (forceCheck || sampleInterNr == checkSample)
            {
                forceCheck = false;
                sampleInterNr = 0;
                res = CheckForStopTerm(msg);

                if (res == AdaptiveStatisticalStopKeyAnalyserResult.StopKeyPostive)
                    forceCheck = true; //force the next sample to get measured
                //recalcSampleCounter++;
            }

            if (recalcSampleCounter == Constants.AdaptiveStatisticalStopKeyAnalyser_recalcSampleRateFrequency)
            {
                RecalcSampleRate();
                recalcSampleCounter = 0;                
            }

            return res;
        }

        private AdaptiveStatisticalStopKeyAnalyserResult CheckForStopTerm(TextMessage msg)
        {
            try
            {
                if (rx_stopTerm.Match(msg.messageText).Success)
                {
                    UpdateStatistic(3);
                    return AdaptiveStatisticalStopKeyAnalyserResult.StopKeyPostive;
                }

                UpdateStatistic(1);
                return AdaptiveStatisticalStopKeyAnalyserResult.StopKeyNegative;
            }
            catch
            {
                UpdateStatistic(2);
                return AdaptiveStatisticalStopKeyAnalyserResult.Untested;
            }
        }

        private void UpdateStatistic(byte sampleValue)  //1==no stop term recognizes; 3==stop term recognized; 2==check failed, take the avg
        {
            isWindowFull = isWindowFull || statisticalWindowWidth == Constants.AdaptiveStatisticalStopKeyAnalyser_statisticalWindowWidth;
            if (isWindowFull)
            {   
                //e.g. Window=1000 -> NewValue = Oldvalue * 999/1000 + sampleValue * 1/1000
                //statisticalWindowWidthMinusOne = Constants.AdaptiveStatisticalStopKeyAnalyser_statisticalWindowWidth - 1;
                //statisticalWindowWidthRecip = 1 / Constants.AdaptiveStatisticalStopKeyAnalyser_statisticalWindowWidth;
                //statisticalWindowWidthMinusOneRel = statisticalWindowWidthMinusOne / Constants.AdaptiveStatisticalStopKeyAnalyser_statisticalWindowWidth;
                statisticalWindowData = statisticalWindowData * statisticalWindowWidthMinusOneRel /*old statistic data*/
                                            + sampleValue * statisticalWindowWidthRecip;          /*input sample data*/
            }
            else
            {
                statisticalWindowWidth++;

                //e.g. MaxWindow=1000, CurrentWindow = 200 -> NewValue = Oldvalue * 199/200 + sampleValue * 1/200
                statisticalWindowData = statisticalWindowData * ((1.0f * statisticalWindowWidth - 1) / statisticalWindowWidth) /*old statistic data*/
                                            + sampleValue * (1 / (1.0f * statisticalWindowWidth)); /*input sample data*/
            }
        }

        private void RecalcSampleRate()  //recalc checkSample variable based on statisticalWindowData
        {
            //if statisticalWindowData == 3 --> ALL measured texts contained stop terms -> checkSample should be 1
            //if statisticalWindowData == 1 --> NONE measured text contained stop terms -> checkSample should be 1000; in between there should be a non linear function

            for (float f = 3.0f; f >= 1.0f; f -= 0.1f)
                if (statisticalWindowData.InRange(f, f + 0.1f))
                {
                    //float y = 3.0f * 1 / f; //1..3
                    //checkSample = ((y - 1) * (1000 / 2) + 1).Int(); //(1 + f * 10).Int();

                    //function: y=f(x)= -125*(x^2) + 1000  --> https://rechneronline.de/funktionsgraphen/ 
                    checkSample = Math.Max(1, (-125f * Math.Pow(f, 2) + 1000).Int());
                    break;
                }
        }
    }

    public enum AdaptiveStatisticalStopKeyAnalyserResult
    {
        StopKeyPostive,
        StopKeyNegative,
        Untested,
        Unevolved
    }
}
