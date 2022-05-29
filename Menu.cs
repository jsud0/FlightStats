using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFS;
using SFS.UI;
//using SFS.Variables;
using SFS.World;

namespace FlightInfo
{
    public class Menu : MonoBehaviour
    {
        // public GUIStyle style;
        public static Rect windowRect = new Rect(10, Screen.height / 2 + 120, 300, 240);
        public double ap, pe;
        public string t_ap, t_pe;
        public double a, b, ra, rp, r, e, l, v, m, ma, ma2, ea, mm, period, angle, sangle;
        public double vy, vx, rvy, rvx, langle, langleFlipped;
        public double height_1, height_2, old_v;
        public double heightTerrain;
        public Orbit orbit;
        public int line;

        public bool ascending;
        // a = smaa
        // b = smia
        // ra = ap+pr
        // rp = pe+pr
        // r = radius
        // e = ecc
        // l = slr
        // v = true anomaly
        // m = planet mass
        // ma = mean anomaly
        // ea = ecc anomaly
        // mm = mean motion


        public string UnitLength(double l)
        {
            if (l >= 1000000)
                return String.Format("{0:n}", l / 1000) + " km";
            return String.Format("{0:n}", l) + " m";
        }

        public string UnitSpeed(double v)
        {
            if (v >= 1000000)
                return String.Format("{0:n}", v / 1000) + " km/s";
            return String.Format("{0:n}", v) + " m/s";
        }

        public string UnitTime(double t)
        {
            string ys, ds, hs, ms, ss;
            int y = (int)Math.Floor(t / 31536000);
            int d = (int)Math.Floor((t - (31536000 * y)) / 86400);
            int h = (int)Math.Floor((t - (31536000 * y) - (86400 * d)) / 3600);
            int m = (int)Math.Floor((t - (31536000 * y) - (86400 * d) - (3600 * h)) / 60);
            double s =              (t - (31536000 * y) - (86400 * d) - (3600 * h) - (60 * m));
            ss = s.ToString("#0.0") + "s";
            ms = null;
            hs = null;
            ds = null;
            ys = null;
            if (t >= 60)
            {
                ss = s.ToString("00.0") + "s";
                ms = m.ToString() + "m";
            }
            if (t >= 3600)
            {
                ss = s.ToString("00.0") + "s";
                ms = m.ToString("00") + "m";
                hs = h.ToString() + "h";
            }
            if (t >= 86400)
            {
                ss = s.ToString("00.0") + "s";
                ms = m.ToString("00") + "m";
                hs = h.ToString("00") + "h";
                ds = d.ToString() + "d";
            }
            if (t >= 31536000)
            {
                ss = s.ToString("00.0") + "s";
                ms = m.ToString("00") + "m";
                hs = h.ToString("00") + "h";
                ds = d.ToString("000") + "d";
                ys = y.ToString() + "y";
            }
            string[] output = { ys, ds, hs, ms, ss};
            return String.Join(" ", output);
        }

        public void windowFunc(int windowID)
        {
            line = 1;
            // style.normal.textColor = new Color(0.46f, 1f, 0.96f);
            PrintStats("Apoapsis", UnitLength(ap));
            PrintStats("Time to Apospsis", t_ap);
            PrintStats("Periapsis", UnitLength(pe));
            PrintStats("Time to Periapsis", t_pe);
            PrintStats("Eccentricity", e.ToString(5, true));
            PrintLine();
            PrintStats("Height (Terrain)", UnitLength(heightTerrain));
            PrintStats("Vertical Velocity", UnitSpeed(rvy));
            PrintStats("Horizontal Velocity", UnitSpeed(rvx));
            PrintStats("Angle", sangle.ToString(1, true) + "°");

            GUI.DragWindow();
        }


        public void OnGUI()
        {
            // ASoD
            var player = (PlayerController.main.player.Value as Rocket);
            var currentRocket = GameManager.main.rockets[GameManager.main.rockets.IndexOf(player)];
            m = currentRocket.location.planet.Value.mass;
            r = currentRocket.location.Value.Radius;
            var sma = m / -(2.0 * (Math.Pow(currentRocket.location.velocity.Value.magnitude, 2.0) / 2.0 - m / r));
            Double3 @double = Double3.Cross(currentRocket.location.position, currentRocket.location.velocity);
            Double2 double2 = (Double2)(Double3.Cross((Double3)currentRocket.location.velocity.Value, @double) / m) - currentRocket.location.position.Value.normalized;
            e = double2.magnitude;
            
            //langle -- l means location (angle made from location)
            var location = currentRocket.physics.location;
            langle = Math.Atan2(location.position.Value.y, location.position.Value.x);
            langleFlipped = Kepler.ToTauRange(Math.Atan2(location.position.Value.x, location.position.Value.y));

            //bool success;
            Orbit o = new Orbit(new Location(location.planet, location.position, location.velocity), true, false);

            ap = sma * (1.0 + e) - currentRocket.location.planet.Value.Radius;
            pe = o.periapsis - currentRocket.location.planet.Value.Radius;
            ra = sma * (1.0 + e);
            rp = o.periapsis;

            // l = 2 * (ra * rp) / (ra + rp);
            // v = Kepler.GetTrueAnomalyAtRadius(r, l, e);

            v = Kepler.ToTauRange(langle - double2.AngleRadians);
            v = Kepler.ToTauRange(v * -o.direction);

            // Debug.Log(v);
            // ma = Kepler.GetMeanAnomaly(e, v);
            // ea = Kepler.GetEccentricAnomalyFromTrueAnomaly(v, e);
            mm = Kepler.GetMeanMotion(sma, m);
            period = Kepler.GetPeriod(sma, m);

            double ap_time, pe_time;
            if (e < 1)
            {
                pe_time = Kepler.GetTimeToPeriapsis(v, e, mm, 1);
                //if (ascending)
                //{
                //    pe_time = period - pe_time;
                //}
                ap_time = period / 2 + pe_time;
                ap_time -= (ap_time > period) ? period : 0;
                pe_time = period / 2 + ap_time;
                pe_time -= (pe_time > period) ? period : 0;
                t_ap = UnitTime(ap_time);
                t_pe = UnitTime(pe_time);
            }
            else
            {
                pe_time = Kepler.GetTimeToPeriapsis(v, e, mm, 1);
                t_ap = "Infinity";
                if (pe_time >= 0)
                {
                    // pe_time = Kepler.GetTimeToPeriapsis(v, e, mm, 1);
                    t_pe = UnitTime(pe_time);
                }
                else t_pe = "Infinity";
                
            }

            heightTerrain = currentRocket.location.planet.Value.GetTerrainHeightAtAngle(Math.Atan2(location.position.Value.y, location.position.Value.x));
            heightTerrain = r - currentRocket.location.planet.Value.Radius - heightTerrain;
            
            // ASoD
            double trueangle = (double)currentRocket.partHolder.transform.eulerAngles.z;
            trueangle = Kepler.ToTauRange(trueangle * Math.PI / 180) * 180 / Math.PI;
            if(trueangle > 180) { trueangle = trueangle - 360; }
            trueangle = -trueangle;
               
            //sangle -- s means subtracted
            sangle = trueangle - langleFlipped * 180 / Math.PI;
            sangle = Kepler.ToTauRange(sangle * Math.PI / 180) * 180 / Math.PI;
            if(sangle > 180) { sangle = sangle - 360; }

            vx = currentRocket.location.velocity.Value.x;
            vy = currentRocket.location.velocity.Value.y;
            //Debug.Log(vx+" "+vy);
            //Debug.Log(Math.Atan2(vx, vy)*180/Math.PI);

            double num = Math.Cos(langleFlipped);
            double num2 = Math.Sin(langleFlipped);
            rvx = vx * num - vy * num2;
            rvy = vx * num2 + vy * num;

            //Debug.Log(rvx+" "+rvy);
            //Debug.Log(Math.Atan2(rvx, rvy)*180/Math.PI);
            //vx = currentRocket.location.velocity.Value.x * (Math.Cos(-langleFlipped) + Math.Sin(-langleFlipped));
            //vy = currentRocket.location.velocity.Value.y * (Math.Cos(-langleFlipped) - Math.Sin(-langleFlipped));
            //Debug.Log(langle*180/Math.PI);
            //Debug.Log(langleFlipped*180/Math.PI);
            //Debug.Log("s> "+sangle+" a> "+langleFlipped+" t> "+trueangle);
            windowRect = GUI.Window(GUIUtility.GetControlID(FocusType.Passive), windowRect, new GUI.WindowFunction(windowFunc), "Flight Info");
        }

        public void PrintStats(string l, string r)
        {
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
            GUI.Label(new Rect(10, line * 20, windowRect.width - 20, 20), l);
            GUI.skin.label.alignment = TextAnchor.LowerRight;
            GUI.Label(new Rect(10, line * 20, windowRect.width - 20, 20), r);
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
            line++;
        }

        public void PrintLine()
        {
            GUI.skin.label.alignment = TextAnchor.LowerCenter;
            GUI.Label(new Rect(10, line * 20, windowRect.width - 20, 20),
            "----------------------------------------------------------------------");
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
            line++;
        }
    }
}