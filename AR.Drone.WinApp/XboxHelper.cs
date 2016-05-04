using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AR.Drone.WinApp
{
    public class XboxHelper
    {
        public List<navOrder> allNavOrders;

        public XboxHelper()
        {
            allNavOrders = new List<navOrder>();
        }

        public List<float> getNavOrders(float LeftX, float LeftY, float RightX, float RightY)
        {
            List<float> navOrders = new List<float>();

            // roll = go right/left, pitch = go forward/backward, yaw = turn right/left, gaz = up/down
            navOrders.Add(DividePlus(RightX)); //roll
            navOrders.Add(DivideMinus(RightY)); //pitch
            navOrders.Add(DividePlus(LeftX)); //yaw
            navOrders.Add(DividePlus(LeftY)); //gaz

            return navOrders;
        }

        private float DivideMinus(float number)
        {
            if (number > -1 && number < 0)
                return 0.25f;
            if (number > 0 && number < 1)
                return -0.25f;
            if (number == 1)
                return -0.5f;
            if (number == -1)
                return 0.5f;
            return 0f;
        }

        private float DividePlus(float number)
        {
            if (number > -1 && number < 0)
                return -0.25f;
            if (number > 0 && number < 1)
                return 0.25f;
            if (number == 1)
                return 0.5f;
            if (number == -1)
                return -0.5f;
            return 0f;
        }
    }

    public class navOrder
    {
        public List<float> orders;
        public string time;
    }

}
