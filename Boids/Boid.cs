

namespace Visio2023Foundry.Boids;
public class Boid
{

    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double AngleXY { get; set; }
    public double AngleXZ { get; set; }
    public double Xvel { get; set; }
    public double Yvel { get; set; }
    public double Zvel { get; set; }
    public string Color { get; set; } = "Yellow";
    public string BoidId { get; set;  }


    public Boid()
    {
        BoidId =  Guid.NewGuid().ToString();
    }

    public Boid(Random rand, double width, double height, string color)
    {
        BoidId =  Guid.NewGuid().ToString();
        X = rand.NextDouble() * width;
        Y = rand.NextDouble() * height;
        Z = 0;
        Xvel = (rand.NextDouble() - .5);
        Yvel = (rand.NextDouble() - .5);
        Zvel = 0;
        Color = color;
        AngleXY = GetAngleXY();
    }
    public Boid(Random rand, double width, double height, double depth, string color)
    {
        BoidId =  Guid.NewGuid().ToString();
        X = rand.NextDouble() * width;
        Y = rand.NextDouble() * height;
        Z = rand.NextDouble() * depth;
        Xvel = (rand.NextDouble() - .5);
        Yvel = (rand.NextDouble() - .5);
        Zvel = (rand.NextDouble() - .5);
        Color = color;
        AngleXY = GetAngleXY();
    }
    public bool MoveXY(int x, int y, double angle)
    {
        (X, Y, AngleXY) = (x, y, angle);
        return true;
    }

    public bool MoveXZ(int x, int z, double angle)
    {
        (X, Z, AngleXZ) = (x, z, angle);
        return true;
    }

    public void MoveForward(double minSpeed = 4, double maxSpeed = 15)
    {
        X += Xvel;
        Y += Yvel;

        var speed = GetSpeed();
        if (speed > maxSpeed)
        {
            Xvel = (Xvel / speed) * maxSpeed;
            Yvel = (Yvel / speed) * maxSpeed;
            Zvel = (Zvel / speed) * maxSpeed;
        }
        else if (speed < minSpeed)
        {
            Xvel = (Xvel / speed) * minSpeed;
            Yvel = (Yvel / speed) * minSpeed;
            Zvel = (Zvel / speed) * minSpeed;
        }

        if (double.IsNaN(Xvel))
            Xvel = 0;
        if (double.IsNaN(Yvel))
            Yvel = 0;
        if (double.IsNaN(Zvel))
            Zvel = 0;
    }

    public (double x, double y) GetPosition(double time)
    {
        return (X + Xvel * time, Y + Yvel * time);
    }

    public void Accelerate(double scale = 1.0)
    {
        Xvel *= scale;
        Yvel *= scale;
        Zvel *= scale;
    }

    public double GetAngleXY()
    {
        if (double.IsNaN(Xvel) || double.IsNaN(Yvel))
            return 0;

        if (Xvel == 0 && Yvel == 0)
            return 0;

        double angle = Math.Atan(Yvel / Xvel) * 180 / Math.PI - 90;
        if (Xvel < 0)
            angle += 180;

        return angle;
    }

    public double GetAngleXZ()
    {
        if (double.IsNaN(Xvel) || double.IsNaN(Zvel))
            return 0;

        if (Xvel == 0 && Zvel == 0)
            return 0;

        double angle = Math.Atan(Zvel / Xvel) * 180 / Math.PI - 90;
        if (Xvel < 0)
            angle += 180;

        return angle;
    }

    public double GetSpeed()
    {
        return Math.Sqrt(Xvel * Xvel + Yvel * Yvel + Zvel * Zvel);
    }

    public double GetDistance(Boid otherBoid)
    {
        double dX = otherBoid.X - X;
        double dY = otherBoid.Y - Y;
        double dZ = otherBoid.Z - Z;
        double dist = Math.Sqrt(dX * dX + dY * dY + dZ * dZ);
        return dist;
    }
}
