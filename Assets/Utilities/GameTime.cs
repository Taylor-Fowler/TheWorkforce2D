using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TheWorkforce
{
    public class GameTime
    {
        private byte _hourMinuteSecondFrame = 0;
        private float _gameStartTime = 0f;
        private float _gameCurrentTime = 0f;


        public GameTime(float startGameTime)
        {
            Application.targetFrameRate = 60;
            _gameStartTime = startGameTime;
        }

        public GameTime(float startGameTime, byte hourMinuteSecondFrame) : this(startGameTime)
        {
            _hourMinuteSecondFrame = hourMinuteSecondFrame;
        }

        #region Public Members
        public void UpdateFrame(float deltaTime)
        {
            _gameCurrentTime += deltaTime;
        }
        #endregion
    }
}
