using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using RubeLoader.Interfaces;

namespace RUBELoaderTest.Entities
{
    public class Character : IJointController, IBodyController
    {
        private const float MaxMotorSpeed = 100f;

        public string JointName { get; set; }
        public Joint Joint { get; set; }

        public string BodyName { get; set; }
        public Body Body { get; set; }

        public WheelJoint Wheel {get { return Joint as WheelJoint; }}

        public void Init()
        {
            Wheel.MotorEnabled = true;
            Wheel.MaxMotorTorque = 100;
        }
        
        public void MoveLeft()
        {
            if (Wheel.MotorSpeed > -MaxMotorSpeed)
            {
                Wheel.MotorSpeed -= 1f;
            }
        }

        public void MoveRight()
        {
            if (Wheel.MotorSpeed < MaxMotorSpeed)
            {
                Wheel.MotorSpeed += 1f;
            }
        }

        public void Jump()
        {
            Body.ApplyForce(new Vector2(0, -1));
        }

        public void Decay()
        {
            if (Wheel.MotorSpeed > 0) Wheel.MotorSpeed -= 0.4f;
            if (Wheel.MotorSpeed < 0) Wheel.MotorSpeed += 0.4f;
        }
    }
}
