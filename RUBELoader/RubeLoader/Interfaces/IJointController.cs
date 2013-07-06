using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics.Joints;

namespace RubeLoader.Interfaces
{
    public interface IJointController
    {
        string JointName { get; set; }
        List<Joint> Joints { get; set; }
    }
}
