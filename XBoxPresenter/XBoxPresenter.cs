﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace XBoxPresenter
{
    class XBoxPresenter
    {

        //Previous state of the gamepad
        private XInputState _oldState;
        //Current state of the gamepad
        private XInputState _currentState;

        //We've these to prevent firing the event before the trigger got fully released
        private bool rightTriggerInUse = false;
        private bool leftTriggerInUse = false;

        event EventHandler XBoxControllerStateChanged = null;

        #region XInputDLLFunctions
        [DllImport("xinput1_4.dll")]
        public static extern int XInputGetState
        (
            int dwUserIndex,
            ref XInputState pState
        );
        #endregion

        #region XInputButtonFlags
        public enum ButtonFlags : int
        {
            XINPUT_GAMEPAD_DPAD_UP = 0x0001,
            XINPUT_GAMEPAD_DPAD_DOWN = 0x0002,
            XINPUT_GAMEPAD_DPAD_LEFT = 0x0004,
            XINPUT_GAMEPAD_DPAD_RIGHT = 0x0008,
            XINPUT_GAMEPAD_START = 0x0010,
            XINPUT_GAMEPAD_BACK = 0x0020,
            XINPUT_GAMEPAD_LEFT_THUMB = 0x0040,
            XINPUT_GAMEPAD_RIGHT_THUMB = 0x0080,
            XINPUT_GAMEPAD_LEFT_SHOULDER = 0x0100,
            XINPUT_GAMEPAD_RIGHT_SHOULDER = 0x0200,
            XINPUT_GAMEPAD_A = 0x1000,
            XINPUT_GAMEPAD_B = 0x2000,
            XINPUT_GAMEPAD_X = 0x4000,
            XINPUT_GAMEPAD_Y = 0x8000,
        };
        #endregion

        #region XInputGameState
        public struct XInputGamepad
        {
            public short wButtons;

            public byte bLeftTrigger, bRightTrigger;

            public short sThumbLX, sThumbLY, sThumbRX, sThumbRY;


            public bool IsButtonPressed(int buttonFlags)
            {
                return (wButtons & buttonFlags) == buttonFlags;
            }

            public bool IsButtonPresent(int buttonFlags)
            {
                return (wButtons & buttonFlags) == buttonFlags;
            }

            public void Copy(XInputGamepad source)
            {
                sThumbLX = source.sThumbLX;
                sThumbLY = source.sThumbLY;
                sThumbRX = source.sThumbRX;
                sThumbRY = source.sThumbRY;
                bLeftTrigger = source.bLeftTrigger;
                bRightTrigger = source.bRightTrigger;
                wButtons = source.wButtons;
            }

            public override bool Equals(object xInputGamepad)
            {
                if (xInputGamepad == null || !(xInputGamepad is XInputGamepad)) return false;
                var toCompare = (XInputGamepad)xInputGamepad;
                return sThumbLX == toCompare.sThumbLX && sThumbLY == toCompare.sThumbLY && sThumbRX == toCompare.sThumbRX && sThumbRY == toCompare.sThumbRY && bLeftTrigger == toCompare.bLeftTrigger && bRightTrigger == toCompare.bRightTrigger && wButtons == toCompare.wButtons;
            }
        }
        #endregion

        #region XInputState
        public struct XInputState
        {
            public int PacketNumber;

            public XInputGamepad Gamepad;

            public void Copy(XInputState source)
            {
                PacketNumber = source.PacketNumber;
                Gamepad.Copy(source.Gamepad);
            }

            public override bool Equals(object xInputState)
            {
                if (xInputState == null || (!(xInputState is XInputState))) return false;
                var toCompare = (XInputState)xInputState;
                return PacketNumber == toCompare.PacketNumber && Gamepad.Equals(toCompare.Gamepad);
            }
        }
        #endregion

        public XBoxPresenter()
        {
            XBoxControllerStateChanged += XBoxPresenter_XBoxControllerStateChanged;
            UpdateGameControllerStatus();
        }

        /// <summary>
        /// Infinite loop. We'll only fire the event when the gamepad's state has changed.
        /// </summary>
        public void UpdateGameControllerStatus()
        {
            while (true)
            {
                _oldState.Copy(_currentState);
                XInputGetState(0, ref _currentState);
                if (_oldState.PacketNumber != _currentState.PacketNumber)
                {
                    XBoxControllerStateChanged(this, null);
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Gonna be fired each time the gamepad state changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void XBoxPresenter_XBoxControllerStateChanged(object sender, EventArgs e)
        {
            var currentLeftTrigger = (int)_currentState.Gamepad.bLeftTrigger;
            var currentRightTrigger = (int)_currentState.Gamepad.bRightTrigger;
            
            if (rightTriggerInUse && currentRightTrigger == 0)
                rightTriggerInUse = false;
            if (leftTriggerInUse && currentLeftTrigger == 0)
                leftTriggerInUse = false;

            if (currentRightTrigger != 0 && !rightTriggerInUse)
            {
                rightTriggerInUse = true;
                System.Windows.Forms.SendKeys.SendWait("{RIGHT}");
                Console.WriteLine("RIGHT");
            }

            if (currentLeftTrigger != 0 && !leftTriggerInUse)
            {
                leftTriggerInUse = true;
                System.Windows.Forms.SendKeys.SendWait("{LEFT}");
                Console.WriteLine("LEFT");
            }

            if (_currentState.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_A))
                System.Windows.Forms.SendKeys.SendWait("B");

            if (_currentState.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_START))
                System.Windows.Forms.SendKeys.SendWait("+{F5}");

            if (_currentState.Gamepad.IsButtonPressed((int)ButtonFlags.XINPUT_GAMEPAD_BACK))
                System.Windows.Forms.SendKeys.SendWait("{ESC}");
        }

        static void Main(string[] args)
        {
            new XBoxPresenter();
        }
    }
}
