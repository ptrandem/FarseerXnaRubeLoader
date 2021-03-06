﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics.Dynamics;

namespace RubeLoader.Interfaces
{
    public interface IBodyController
    {
        string BodyName { get; set; }
        List<Body> Bodies { get; set; }
    }
}
