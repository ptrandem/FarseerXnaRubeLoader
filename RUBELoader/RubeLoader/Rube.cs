using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;


namespace RubeLoader
{
    public class Rube
    {
        private List<Fixture> _fixtures;
        private List<Joint> _joints;
        private readonly string _jsonString;
        private readonly dynamic _json;
        private readonly Dictionary<string, Body> _bodies;


        public World World { get; private set; }

        public Rube(string rubePath)
        {
            _bodies = new Dictionary<string,Body>();
            _fixtures = new List<Fixture>();
            _joints = new List<Joint>();

            _jsonString = File.ReadAllText(rubePath);

            _json = JObject.Parse(_jsonString);
            float gx =  _json.gravity.x;
            float gy =  _json.gravity.y;
            World = new World(new Vector2(gx, gy));
            foreach (var bodyData in _json.body)
            {
                Body body = BodyFactory.CreateBody(World, new Vector2((float)bodyData.position.x, (float)bodyData.position.y), GetPropertyOrDefault(bodyData.Angle, 0f));
                body.AngularVelocity = GetPropertyOrDefault(bodyData.fixedRotation, 0f);
                body.FixedRotation = GetPropertyOrDefault(bodyData.fixedRotation, false);
                body.Awake = GetPropertyOrDefault(bodyData.awake, true);
                body.BodyType = (BodyType) (GetPropertyOrDefault(bodyData.type, (int)BodyType.Dynamic));
                _bodies.Add(GetPropertyOrDefault(bodyData.name, ""), body);
            }
        }

        private static T GetPropertyOrDefault<T>(dynamic value, T defaultValue)
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
