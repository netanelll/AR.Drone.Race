using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XInputDotNetPure;

namespace AR.Drone.WinApp
{
    public class XboxHelper
    {
        public List<navOrder> allNavOrders;

        public XboxHelper()
        {
            allNavOrders = new List<navOrder>();
        }

        public List<float> getNavOrders(ButtonState turnLeft, ButtonState turnRight, ButtonState goUp, ButtonState goDown,
            float RightX, float RightY)
        {
            List<float> navOrders = new List<float>();

            // roll = go right/left, pitch = go forward/backward, yaw = turn right/left, gaz = up/down
            navOrders.Add(DividePlus(RightX)); //roll
            navOrders.Add(DivideMinus(RightY)); //pitch
            navOrders.Add(GetLeftRight(turnLeft, turnRight)); //yaw
            navOrders.Add(GetUpDown(goUp, goDown)); //gaz

            return navOrders;
        }

        private float GetUpDown(ButtonState goUp, ButtonState goDown)
        {
            if (goUp == ButtonState.Pressed)
                return 0.25f;
            if (goDown == ButtonState.Pressed)
                return -0.25f;
            return 0f;
        }

        private float GetLeftRight(ButtonState turnLeft, ButtonState turnRight)
        {
            if (turnLeft == ButtonState.Pressed)
                return -0.5f;
            if (turnRight == ButtonState.Pressed)
                return 0.5f;
            return 0f;
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
