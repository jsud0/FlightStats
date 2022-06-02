using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FlightInfo
{
    public class Units : MonoBehaviour
    {
        public string UnitLength(double l)
        {
            if (l >= 1000000)
                return String.Format("{0:n}", l / 1000) + " km";
            return String.Format("{0:n}", l) + " m";
        }

        public string UnitSpeed(double v)
        {
            if (v >= 100000)
                return String.Format("{0:n}", v / 1000) + " km/s";
            return String.Format("{0:n}", v) + " m/s";
        }

        // mess code
        public string UnitTime(double t)
        {
            if (t / 31536000 >= ulong.MaxValue)
                return (t / 31536000).ToString() + "y";
            string ys, ds, hs, ms, ss;
            ulong y = (ulong)Math.Floor(t / 31536000);
            ulong d = (ulong)Math.Floor((t - (31536000 * y)) / 86400);
            ulong h = (ulong)Math.Floor((t - (31536000 * y) - (86400 * d)) / 3600);
            ulong m = (ulong)Math.Floor((t - (31536000 * y) - (86400 * d) - (3600 * h)) / 60);
            double s = (t - (31536000 * y) - (86400 * d) - (3600 * h) - (60 * m));
            ss = s.ToString("#0.0") + "s";
            ms = hs = ds = ys = null;
            if (t >= 60)
            {
                ss = s.ToString("00.0") + "s";
                ms = m.ToString() + "m";
            }
            if (t >= 3600)
            {
                ms = m.ToString("00") + "m";
                hs = h.ToString() + "h";
            }
            if (t >= 86400)
            {
                hs = h.ToString("00") + "h";
                ds = d.ToString() + "d";
            }
            if (t >= 31536000)
            {
                ds = d.ToString("000") + "d";
                ys = y.ToString() + "y";
            }
            string[] output = { ys, ds, hs, ms, ss };
            return String.Join(" ", output);
        }
    }
}
