using System;
using SFS.World;
using UnityEngine;

namespace FlightInfo
{
    public class Menu : MonoBehaviour
    {   
        // height = line * 20 + 5
        public static Rect windowRectGene = new Rect(10, Screen.height / 2 - 150, 300, 125);
        public static Rect windowRectSurf = new Rect(10, Screen.height / 2 - 0, 300, 105);
        public double ap, pe;
        public string t_ap, t_pe;
        public double ra, rp,
                      r, e, v, m, mm,
                      period, angle, sangle;
        public double vy, vx, rvy, rvx, langle,
                      langleFlipped;
        public double height_1, height_2, old_v;
        public double heightTerrain;
        public Orbit orbit;
        public int lineGene, lineSurf;

        public bool ascending;

        Units units = new Units();

        // ra = ap+pr
        // rp = pe+pr
        // r = radius
        // e = ecc
        // v = true anomaly
        // m = planet mass
        // mm = mean motion

        public void windowFuncGene(int windowID)
        {
            lineGene = 0;
            lineGene++; PrintStats(lineGene, "Apoapsis", units.UnitLength(ap), windowRectGene);
            lineGene++; PrintStats(lineGene, "Time to Apospsis", t_ap, windowRectGene);
            lineGene++; PrintStats(lineGene, "Periapsis", units.UnitLength(pe), windowRectGene);
            lineGene++; PrintStats(lineGene, "Time to Periapsis", t_pe, windowRectGene);
            lineGene++; PrintStats(lineGene, "Eccentricity", e.ToString(5, true), windowRectGene);
            GUI.DragWindow();
        }
        public void windowFuncSurf(int windowID)
        {
            lineSurf = 0;
            lineSurf++; PrintStats(lineSurf, "Height (Terrain)", units.UnitLength(heightTerrain), windowRectSurf);
            lineSurf++; PrintStats(lineSurf, "Vertical Velocity", units.UnitSpeed(rvy), windowRectSurf);
            lineSurf++; PrintStats(lineSurf, "Horizontal Velocity", units.UnitSpeed(rvx), windowRectSurf);
            lineSurf++; PrintStats(lineSurf, "Angle", sangle.ToString(1, true) + "°", windowRectSurf);
            GUI.DragWindow();
        }

        public void OnGUI()
        {
            // ASoD
            var player = (PlayerController.main.player.Value as Rocket);
            var currentRocket = GameManager.main.rockets[GameManager.main.rockets.IndexOf(player)];
            var location = currentRocket.physics.location;
            var position = location.position;
            m = location.planet.Value.mass;
            r = location.Value.Radius;
            var sma = m / -(2.0 * (Math.Pow(location.velocity.Value.magnitude, 2.0) / 2.0 - m / r));
            Double3 @double = Double3.Cross(position, location.velocity);
            Double2 double2 = (Double2)(Double3.Cross((Double3)location.velocity.Value, @double) / m) - position.Value.normalized;
            e = double2.magnitude;
            
            //langle -- l means location (angle made from location)
            langle = Math.Atan2(position.Value.y, position.Value.x);
            langleFlipped = Kepler.ToTauRange(Math.Atan2(position.Value.x, position.Value.y));

            Orbit o = new Orbit(new Location(location.planet, position, location.velocity), true, false);

            ap = sma * (1.0 + e) - location.planet.Value.Radius;
            pe = o.periapsis - location.planet.Value.Radius;
            ra = sma * (1.0 + e);
            rp = o.periapsis;

            v = Kepler.ToTauRange(langle - double2.AngleRadians * -o.direction);

            mm = Kepler.GetMeanMotion(sma, m);
            period = Kepler.GetPeriod(sma, m);
            double ap_time, pe_time;
            if (e < 1)
            {
                pe_time = Kepler.GetTimeToPeriapsis(v, e, mm, 1);
                ap_time = period / 2 + pe_time;
                ap_time -= (ap_time > period) ? period : 0;
                pe_time = period / 2 + ap_time;
                pe_time -= (pe_time > period) ? period : 0;
                t_ap = units.UnitTime(ap_time);
                t_pe = units.UnitTime(pe_time);
            }
            else
            {
                pe_time = Kepler.GetTimeToPeriapsis(v, e, mm, 1);
                t_ap = "Infinity";
                if (pe_time >= 0)
                {
                    t_pe = units.UnitTime(pe_time);
                }
                else t_pe = "Infinity";
            }

            heightTerrain = location.planet.Value.GetTerrainHeightAtAngle(Math.Atan2(position.Value.y, position.Value.x));
            heightTerrain = r - location.planet.Value.Radius - heightTerrain;
            
            // ASoD
            double trueangle = (double)currentRocket.partHolder.transform.eulerAngles.z;
            trueangle = Kepler.ToTauRange(trueangle * Math.PI / 180) * 180 / Math.PI;
            if (trueangle > 180) { trueangle = trueangle - 360; }
            trueangle = -trueangle;
               
            //sangle -- s means subtracted
            sangle = trueangle - langleFlipped * 180 / Math.PI;
            sangle = Kepler.ToTauRange(sangle * Math.PI / 180) * 180 / Math.PI;
            if(sangle > 180) { sangle = sangle - 360; }

            vx = location.velocity.Value.x;
            vy = location.velocity.Value.y;

            double num = Math.Cos(langleFlipped);
            double num2 = Math.Sin(langleFlipped);
            rvx = vx * num - vy * num2;
            rvy = vx * num2 + vy * num;

            //if(GetComponent<Main>().gene)
                windowRectGene = GUI.Window(GUIUtility.GetControlID(FocusType.Passive),
                    windowRectGene,
                    new GUI.WindowFunction(windowFuncGene), "General");
            //if(GetComponent<Main>().surf)
                windowRectSurf = GUI.Window(GUIUtility.GetControlID(FocusType.Passive),
                    windowRectSurf,
                    new GUI.WindowFunction(windowFuncSurf), "Surface");
        }

        public void PrintStats(int line, string l, string r, Rect rect)
        {
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
            GUI.Label(new Rect(10, line * 20, rect.width - 20, 20), l);
            GUI.skin.label.alignment = TextAnchor.LowerRight;
            GUI.Label(new Rect(10, line * 20, rect.width - 20, 20), r);
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
        }

        /*
        public void PrintLine()
        {
            GUI.skin.label.alignment = TextAnchor.LowerCenter;
            GUI.Label(new Rect(10, line * 20, windowRect.width - 20, 20),
            "----------------------------------------------------------------------");
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
            line++;
        }
        */
    }
}