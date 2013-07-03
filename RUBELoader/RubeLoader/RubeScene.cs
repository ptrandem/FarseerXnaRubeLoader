using System;
using System.Collections.Generic;
using System.IO;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework;

namespace RubeLoader
{
    public class RubeScene
    {
        private readonly string _jsonString;
        private readonly dynamic _json;
        private readonly List<Body> _bodies;
        private readonly List<Joint> _joints;
        private readonly Dictionary<string, Texture2D> _textures;
        private readonly List<BodyImage> _images;
        private readonly ContentManager _content;
        private readonly GraphicsDevice _device;
        private readonly SpriteBatch _spriteBatch;
        
        public World World { get; private set; }

        public RubeScene(string rubePath, ContentManager content, GraphicsDevice device)
        {
            _spriteBatch = new SpriteBatch(device);
            _bodies = new List<Body>();
            _joints = new List<Joint>();
            _textures = new Dictionary<string, Texture2D>();
            _images = new List<BodyImage>();
            _content = content;
            _device = device;

            _jsonString = File.ReadAllText(rubePath);

            _json = JObject.Parse(_jsonString);
            World = new World(InvertY(GetVector2(_json.gravity)));
            foreach (var b in _json.body)
            {
                Body body = BodyFactory.CreateBody(World, InvertY(GetVector2(b.position)), GetValue(b.Angle, 0f));
                body.AngularVelocity = GetValue(b.fixedRotation, 0f);
                body.FixedRotation = GetValue(b.fixedRotation, false);
                body.Rotation = InvertYRadians(GetValue(b.angle, 0f));
                body.Awake = GetValue(b.awake, true);
                body.BodyType = (BodyType)(GetValue(b.type, (int)BodyType.Dynamic));

                foreach (var f in b.fixture)
                {
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
                        fixture = FixtureFactory.AttachPolygon(verts, GetValue(f.density, 1f), body);
                    }

                    if (fixture != null)
                    {
                        fixture.Friction = friction;
                    }
                }
                var bodyData = new BodyData {Name = GetValue(b.Name, "")};
                body.UserData = bodyData;
                _bodies.Add(body);
            }

            if (_json.joint != null)
            {
                foreach (var j in _json.joint)
                {
                    bool jointAttachedToWorld = false;
                    Joint joint = null;
                    string type = j.type;
                    int bodyAIndex = GetValue(j.bodyA, 0);
                    int bodyBIndex = GetValue(j.bodyB, 0);
                    var anchorA = InvertY(GetVector2(j.anchorA));
                    var anchorB = InvertY(GetVector2(j.anchorB));
                    switch (type)
                    {
                        case "wheel":
                            var wheelAxis = InvertY(new Vector2(GetValue(j.localAxisA.x, 0f), GetValue(j.localAxisA.y, 0f)));
                            joint = new WheelJoint(_bodies[bodyAIndex], _bodies[bodyBIndex], _bodies[bodyBIndex].Position + anchorB, wheelAxis);
                            ((WheelJoint) joint).MotorEnabled = GetValue(j.enableMotor, false);
                            ((WheelJoint) joint).MaxMotorTorque = GetValue(j.maxMotorTorque, 0f);
                            ((WheelJoint) joint).MotorSpeed = GetValue(j.motorSpeed, 0f);
                            ((WheelJoint) joint).SpringDampingRatio = GetValue(j.springDampingRatio, 0.5f);
                            ((WheelJoint) joint).CollideConnected = GetValue(j.collideConnected, false);
                            ((WheelJoint) joint).SpringFrequencyHz = GetValue(j.springFrequency, 20f);
                            break;

                        case "revolute":
                            joint = new RevoluteJoint(_bodies[bodyAIndex], _bodies[bodyBIndex], anchorB, false);
                            ((RevoluteJoint) joint).CollideConnected = GetValue(j.collideConnected, false);
                            ((RevoluteJoint)joint).ReferenceAngle = -InvertYRadians(GetValue(j.refAngle, 0f));
                            ((RevoluteJoint) joint).MotorEnabled = GetValue(j.enableMotor, false);
                            ((RevoluteJoint) joint).MaxMotorTorque = GetValue(j.maxMotorTorque, 0f);
                            ((RevoluteJoint) joint).MotorSpeed = GetValue(j.motorSpeed, 0f);
                            ((RevoluteJoint)joint).SetLimits(InvertYRadians(GetValue(j.upperLimit, 0f)), InvertYRadians(GetValue(j.lowerLimit, 0f)));
                            ((RevoluteJoint) joint).LimitEnabled = GetValue(j.enableLimit, false);
                            break;

                        case "prismatic":
                            var localAxisA = InvertY(GetVector2(j.localAxisA));
                            var pAxis = (AngleToVector(_bodies[bodyAIndex].Rotation - VectorToAngle(localAxisA)));
                            pAxis.Normalize();
                            joint = JointFactory.CreatePrismaticJoint(_bodies[bodyAIndex], _bodies[bodyBIndex], anchorB, pAxis);
                            ((PrismaticJoint) joint).LocalAnchorA = anchorA;
                            ((PrismaticJoint) joint).MotorEnabled = GetValue(j.enableMotor, false);
                            ((PrismaticJoint) joint).MaxMotorForce = GetValue(j.maxMotorForce, 0f);
                            ((PrismaticJoint) joint).MotorSpeed = GetValue(j.motorSpeed, 0f);
                            ((PrismaticJoint) joint).LimitEnabled = GetValue(j.enableLimit, false);
                            ((PrismaticJoint) joint).SetLimits(GetValue(j.lowerLimit, 0f), GetValue(j.upperLimit, 0f));
                            ((PrismaticJoint)joint).CollideConnected = GetValue(j.collideConnected, false);
                            ((PrismaticJoint)joint).ReferenceAngle = InvertYRadians(GetValue(j.refAngle, 0f));
                            break;

                        case "distance":
                            joint = new DistanceJoint(_bodies[bodyAIndex], _bodies[bodyBIndex], anchorA, anchorB, false);
                            ((DistanceJoint) joint).DampingRatio = GetValue(j.dampingRatio, 0f);
                            ((DistanceJoint) joint).Frequency = GetValue(j.frequency, 0f);
                            ((DistanceJoint)joint).Length = GetValue(j.length, 0f);
                            break;

                        case "rope":
                            joint = new RopeJoint(_bodies[bodyAIndex], _bodies[bodyBIndex], anchorA, anchorB);
                            ((RopeJoint) joint).CollideConnected = GetValue(j.collideConnected, false);
                            ((RopeJoint) joint).MaxLength = GetValue(j.maxLength, 1f);
                            break;

                        case "weld":
                            //jointAttachedToWorld = true;
                            //joint = JointFactory.CreateWeldJoint(World, _bodies[bodyAIndex], _bodies[bodyBIndex], anchorA, anchorB);
                            joint = new WeldJoint(_bodies[bodyAIndex], _bodies[bodyBIndex], _bodies[bodyAIndex].Position + anchorA, _bodies[bodyBIndex].Position + anchorB);
                            ((WeldJoint)joint).CollideConnected = GetValue(j.collideConnected, false);
                            ((WeldJoint) joint).DampingRatio = GetValue(j.DampingRatio, 0f);
                            ((WeldJoint) joint).FrequencyHz = GetValue(j.frequency, 0f);
                            //((WeldJoint) joint).ReferenceAngle = GetValue(j.refAngle, 0f);
                            break;

                        case "motor":
                            joint = new MotorJoint(_bodies[bodyAIndex], _bodies[bodyBIndex]);
                            ((MotorJoint) joint).LinearOffset = anchorA;
                            ((MotorJoint) joint).AngularOffset = InvertYRadians(GetValue(j.refAngle, 0f));
                            ((MotorJoint) joint).MaxForce = GetValue(j.maxForce, 0f);
                            ((MotorJoint) joint).MaxTorque = GetValue(j.maxTorque, 0f);
                            ((MotorJoint) joint).CollideConnected = GetValue(j.collideConnected, false);
                            break;
                    }

                    if (joint != null)
                    {
                        if (!jointAttachedToWorld)
                        {
                            World.AddJoint(joint);
                        }
                        _joints.Add(joint);
                    }
                }
            }

            if (_json.image != null)
            {
                foreach (var i in _json.image)
                {
                    if (i.file != null)
                    {
                        var path = (string) i.file;
                        var n = path.LastIndexOf('/') + 1;
                        var m = path.LastIndexOf('.');
                        var textureName = path.Substring(n, m - n);
                        if (!_textures.ContainsKey(textureName))
                        {
                            var texture = _content.Load<Texture2D>(textureName);
                            _textures.Add(textureName, texture);
                        }

                        var image = new BodyImage
                            {
                                Texture = _textures[textureName],
                                Body = _bodies[(int) GetValue(i.body, 0)],
                                Center = GetVector2(i.center),
                                Corners = GetVectors(i.corners),
                                Name = i.name,
                                Opacity = i.opacity,
                                Scale = i.scale
                            };

                        _images.Add(image);
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            World.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 60f)));
        }

        public void Draw()
        {
            
            _spriteBatch.Begin();

            //foreach (var body in _bodies)
            //{
            //    foreach (var fixture in body.FixtureList)
            //    {
            //        Vertices verts;
            //        switch (fixture.ShapeType)
            //        {
            //            case ShapeType.Circle:
            //                var circle = fixture.Shape as CircleShape;
            //                _spriteBatch.DrawCircle(ConvertUnitsA.ToDisplayUnits(body.Position),
            //                                        ConvertUnitsA.ToDisplayUnits(circle.Radius), 40, Color.LightBlue, 1f);
            //                break;

            //            case ShapeType.Chain:
            //                var chain = fixture.Shape as ChainShape;
            //                DrawPolygon(_spriteBatch, chain.Vertices, body.Position);
            //                break;

            //            case ShapeType.Polygon:
            //                var poly = fixture.Shape as PolygonShape;
            //                DrawPolygon(_spriteBatch, poly.Vertices, body.Position);

            //                break;
            //            //case ShapeType.Edge:
            //            //    var edge = fixture.Shape as EdgeShape;
            //            //    DrawPolygon(_spriteBatch, edge.Vertex0, edge.Vertex1 body.Position);
            //            //    break;
            //        }
            //    }
            //}

            foreach (var image in _images)
            {
                _spriteBatch.Draw(image.Texture, ConvertUnitsA.ToDisplayUnits(image.Body.Position),
                                           null, Color.White, image.Body.Rotation, image.Center, image.Scale, SpriteEffects.None,
                                           0f);

            }
            
            _spriteBatch.End();
        }

        private static Vector2 AngleToVector(float angle)
        {
            var vec = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            vec.Normalize();
            return vec;
        }

        private static float VectorToAngle(Vector2 vector)
        {
            return (float)Math.Atan2(vector.Y, vector.X);
        }

        private static float InvertYRadians(float value)
        {
            return (float)(2 * Math.PI) - value;
        }

        private static Vector2 InvertY(Vector2 vec)
        {
            return new Vector2(vec.X, InvertY(vec.Y));
        }

        private static float InvertY(float y)
        {
            return 0 - y;
        }

        private void DrawPolygon(SpriteBatch spriteBatch, Vertices verts, Vector2 origin)
        {
            var firstPoint = new Vector2();
            for (var i = 0; i < verts.Count -1; i++)
            {
                var start = ConvertUnitsA.ToDisplayUnits(verts[i]) + ConvertUnitsA.ToDisplayUnits(origin);
                var end = ConvertUnitsA.ToDisplayUnits(verts[i+1]) + ConvertUnitsA.ToDisplayUnits(origin);
                spriteBatch.DrawLine(start, end, Color.LightBlue, 1f);

                if (i == 0)
                {
                    firstPoint = start;
                }
                if (i == verts.Count - 2)
                {
                    spriteBatch.DrawLine(end, firstPoint, Color.LightBlue, 1f);
                }
            }
            
        }


        private static Vertices GetVertices(dynamic vertsArray, bool invertY = true)
        {
            var vertices = new Vertices();
            var list = GetVectors(vertsArray, invertY);
            foreach (var item in list)
            {
                vertices.Add(item);
            }
            return vertices;
        }

        private static IEnumerable<Vector2> GetVectors(dynamic vertsArray, bool invertY = true)
        {
            var vectors = new List<Vector2>();
            var count = vertsArray.x.Count;
            for (int i = 0; i < count; i++)
            {
                var vec = new Vector2((float) vertsArray.x[i], (float) vertsArray.y[i]);
                vectors.Add(invertY ? InvertY(vec) : vec);
            }
            return vectors;
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

        private static Vector2 GetVector2(dynamic value)
        {
            var v = new Vector2();
            if (value != null)
            {
                if ((value as JToken).Type == JTokenType.Object)
                {
                    v = new Vector2(GetValue(value.x, 0f), GetValue(value.y, 0f));
                }
                else
                {
                    v = new Vector2(GetValue(value, 0f), GetValue(value, 0f));
                }
            }

            return v;
        }
    }
}

