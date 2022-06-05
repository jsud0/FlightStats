using System;
using SFS.World;
using UnityEngine;

namespace FlightInfo
{
    public class Menu : MonoBehaviour
    {
        // height = line * 20 + 5
        public static Rect rectGene;
        public static Rect rectSurf;
        public double ap, pe;
        public string t_ap, t_pe;
        public double speed, r, e, v, m, mm, sma,
                      period, trueAngle, craftAngle;
        public double vy, vx, rvy, rvx, angle,
                      angleFlipped;
        public double height, heightTerr;
        public Orbit orbit;

        public bool ascending;

        readonly Units units = new Units();

        // ra = ap+pr
        // rp = pe+pr
        // r = radius
        // e = ecc
        // v = true anomaly
        // m = planet mass
        // mm = mean motion
        
        // messy code top kek
        public void WindowFuncGene(int windowID)
        {
            int l = 0;
            rectGene = new Rect(10, Screen.height / 2f - 0, 300, 165);
            Rect r = rectGene;
            Print(ref l, r, "Height",              units.UnitLength(height) );
            Print(ref l, r, "Velocity",            units.UnitSpeed(speed)   );
            Print(ref l, r, "Apoapsis",            units.UnitLength(ap)     );
            Print(ref l, r, "Time to Apospsis",    t_ap                     );
            Print(ref l, r, "Periapsis",           units.UnitLength(pe)     );
            Print(ref l, r, "Time to Periapsis",   t_pe                     );
            Print(ref l, r, "Eccentricity",        e.ToString(5, true)      );
            GUI.DragWindow();
        }
        public void WindowFuncSurf(int windowID)
        {
            int l = 0;
            rectSurf = new Rect(10, Screen.height / 2f + 2 * Screen.height / 9f, 300, 105);
            Rect r = rectSurf;
            Print(ref l, r, "Height (Terrain)",    units.UnitLength(heightTerr)         );
            Print(ref l, r, "Vertical Velocity",   units.UnitSpeed(rvy)                 );
            Print(ref l, r, "Horizontal Velocity", units.UnitSpeed(rvx)                 );
            Print(ref l, r, "Angle",               craftAngle.ToString(1, true) + "°"   );
            
            GUI.DragWindow();
        }

        public void OnGUI()
        {
            // ASoD
            var player = (PlayerController.main.player.Value as Rocket);
            var currentRocket = GameManager.main.rockets[GameManager.main.rockets.IndexOf(player)];
            var location = currentRocket.physics.location;
            var position = location.position;
            Orbit o = new Orbit(new Location(location.planet, position, location.velocity), true, false);

            Double3 @double = Double3.Cross(position, location.velocity);
            Double2 double2 = (Double2)(Double3.Cross((Double3)location.velocity.Value, @double) / m) - position.Value.normalized;

            m = location.planet.Value.mass;
            r = location.Value.Radius;
            sma = o.sma;
            e = o.ecc;
            
            angle = Math.Atan2(position.Value.y, position.Value.x);
            angleFlipped = Kepler.ToTauRange(Math.Atan2(position.Value.x, position.Value.y));
            
            ap = o.apoapsis - location.planet.Value.Radius;
            pe = o.periapsis - location.planet.Value.Radius;

            v = Kepler.ToTauRange(angle - double2.AngleRadians * -o.direction);

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
                    t_pe = units.UnitTime(pe_time);
                else t_pe = "Infinity";
            }

            height = r - location.planet.Value.Radius;
            heightTerr = location.planet.Value.GetTerrainHeightAtAngle(angle);
            heightTerr = height - heightTerr;
            Debug.Log(SelectableObject.mapObjects.Count);
            // ASoD
            trueAngle = To360Range(currentRocket.partHolder.transform.eulerAngles.z);
            if (trueAngle > 180) { trueAngle -= 360; }
            trueAngle = -trueAngle; 

            craftAngle = To360Range(trueAngle - angleFlipped * 180 / Math.PI);
            if (craftAngle > 180) { craftAngle -= 360; }

            // Surface speed
            speed = location.velocity.Value.magnitude;
            vx = location.velocity.Value.x;
            vy = location.velocity.Value.y;
            double num = Math.Cos(angleFlipped);
            double num2 = Math.Sin(angleFlipped);
            rvx = vx * num - vy * num2;
            rvy = vx * num2 + vy * num;
            
            // UI scaling
            var ratio = Screen.height / 1080f;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(ratio, ratio, 1));
            rectGene = GUI.Window(GUIUtility.GetControlID(FocusType.Passive),
                rectGene, new GUI.WindowFunction(WindowFuncGene), "General");
            rectSurf = GUI.Window(GUIUtility.GetControlID(FocusType.Passive),
                rectSurf, new GUI.WindowFunction(WindowFuncSurf), "Surface");
        }

        public void Print(ref int line, Rect rect, string l, string r)
        {
            line++;
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
            GUI.Label(new Rect(10, line * 20, rect.width - 20, 20), l);
            GUI.skin.label.alignment = TextAnchor.LowerRight;
            GUI.Label(new Rect(10, line * 20, rect.width - 20, 20), r);
            GUI.skin.label.alignment = TextAnchor.LowerLeft;
        }

        public double To360Range(double d)
        {
            if (d < 0)
                d = 360 - d;
            return d % 360;
        }
    }
}