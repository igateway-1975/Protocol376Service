using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol376Service
{
    public class Utility
    {
        public static String GetDateTime(String strData, int position)
        {
            String dateTime = strData.Substring(position + 8, 2) + "-" + strData.Substring(position + 6, 2) + "-" + strData.Substring(position + 4, 2) +
                " " + strData.Substring(position + 2, 2) + ":" + strData.Substring(position, 2);
            return dateTime;
        }

        public static float GetPowerValue(String strData, int position)
        {
            String strPowerValue = strData.Substring(position + 8, 2) + strData.Substring(position + 6, 2) +
                strData.Substring(position + 4, 2) + "." + strData.Substring(position + 2, 2) + strData.Substring(position, 2);
            float value = 0.0f;
            
            if (float.TryParse(strPowerValue, out value))
            {
                return value;
            }

            return -1;
        }

        public static float GetEnergyValue(String strData, int position)
        {
            String strPowerValue = strData.Substring(position + 8, 2) + strData.Substring(position + 6, 2) +
                strData.Substring(position + 4, 2) + "." + strData.Substring(position + 2, 2);
            float value = 0.0f;

            if (float.TryParse(strPowerValue, out value))
            {
                return value;
            }

            return -1;
        }

        public static float GetTemperatureValue(String strData, int position)
        {
            String strPowerValue = strData.Substring(position + 4, 2) + strData.Substring(position + 2, 2) + "." + strData.Substring(position, 2);
            float value = 0.0f;

            if (float.TryParse(strPowerValue, out value))
            {
                return value;
            }

            return -1;
        }

    }
}
