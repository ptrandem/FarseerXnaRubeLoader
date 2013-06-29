using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;


namespace RubeLoader
{
    public class Rube
    {
        private List<Joint> _joints;
        private readonly string _jsonString;
        private readonly dynamic _json;
        private readonly Dictionary<string, Body> _bodies;


        public World World { get; private set; }

        public Rube(string rubePath)
        {
            _bodies = new Dictionary<string, Body>();
            _joints = new List<Joint>();

            _jsonString = File.ReadAllText(rubePath);

            _json = JObject.Parse(_jsonString);
            float gx = _json.gravity.x;
            float gy = _json.gravity.y;
            World = new World(new Vector2(gx, gy));
            foreach (var b in _json.body)
            {
                Body body = BodyFactory.CreateBody(World, new Vector2((float)b.position.x, (float)b.position.y), GetValue(b.Angle, 0f));
                body.AngularVelocity = GetValue(b.fixedRotation, 0f);
                body.FixedRotation = GetValue(b.fixedRotation, false);
                body.Awake = GetValue(b.awake, true);
                body.BodyType = (BodyType)(GetValue(b.type, (int)BodyType.Dynamic));

                foreach (var f in b.fixture)
                {
                    var density = GetValue(f.density, 1f);
                    var friction = GetValue(f.friction, 0.2f);

                    Fixture fixture = null;

                    
                    if (f.chain != null && f.chain.vertices != null)
                    {
                        var verts = GetVertices(f.chain.vertices);
                        fixture = FixtureFactory.AttachChainShape(verts, body);
                    }
                    else if (f.circle != null)
                    {
                        // todo: does f.circle.center map to offset?
                        fixture = FixtureFactory.AttachCircle(GetValue(f.circle.radius, 0f), GetValue(f.density, 1f),
                                                              body);
                    }
                    else if (f.polygon != null)
                    {
                        var verts = GetVertices(f.polygon.vertices);
                        fixture = FixtureFactory.AttachPolygon(verts, GetValue(f.density, 1), body);
                    }

                    if (fixture != null)
                    {
                        fixture.Friction = friction;
                    }
                }
                _bodies.Add(GetValue(b.name, ""), body);
            }
        }

        private static Vertices GetVertices(dynamic vertsArray)
        {
            var vertices = new Vertices();
            var count = vertsArray.x.Count;
            for (int i = 0; i < count; i++)
            {
                vertices.Add(new Vector2((float)vertsArray.x[i], (float)vertsArray.y[i]));
            }
            return vertices;
        }

        private static T GetValue<T>(dynamic value, T defaultValue)
        {
            T result = defaultValue;
            if (value != null)
            {
                result = (T)value;
            }
            return result;
        }
    }
}
