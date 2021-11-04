using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SaveMyLife
{
    public abstract class Utility
    {
        #region Protected attributes
        protected Stopwatch _stopWatch = new Stopwatch();
        protected double _cooldown;
        protected string _affectedKey;
        #endregion

        #region Constructors and public methods
        protected Utility(double cooldown, string affectedKey)
        {
            _cooldown = cooldown;
            _affectedKey = affectedKey;
        }

        //Uses the flask if it's not already being used
        public void Use()
        {
            if (!_stopWatch.IsRunning || _stopWatch.ElapsedMilliseconds > _cooldown)
            {
                SendKeys.SendWait("{"+_affectedKey+"}");
                _stopWatch.Restart();
            }
        }

        public void Reset()
        {
            _stopWatch.Reset();
        }
        #endregion
    }
}
