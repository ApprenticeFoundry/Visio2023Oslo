using System.Drawing;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Extensions;
using FoundryBlazor.Message;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;


// Washington Monument/Coordinates
// 38.8895° N, 77.0353° W

namespace Visio2023Foundry.Boids;

public interface IBoidField
{

}
public class BoidField : IBoidField
{
    public int PredatorCount = 0;
    public double Width { get; private set; }
    public double Height { get; private set; }
    public double Depth { get; private set; }
    public readonly List<Boid> LocalBoids = new();
    public readonly Dictionary<string, Boid> ReflectedBoids = new();
    public readonly Dictionary<string, Boid> ForeignBoids = new();
    private readonly Random Rand = new();

    public bool IsRunning = false;

    private ICommand Command { get; set; }
    private IWorkspace Workspace { get; set; }
    private IDrawing Drawing { get; set; }
    private IArena Arena { get; set; }
    private FoFieldShape2D? FieldShape { get; set; }
    private Rectangle BoidAreaXY { get; set; }
    private Rectangle BoidAreaZY { get; set; }
    private Rectangle BoidAreaXZ { get; set; }

    public BoidField(IWorkspace space, ICommand command)
    {
        Workspace = space;
        Command = command;
        Drawing = Workspace.GetDrawing();
        Arena = Workspace.GetArena();
        var page = Drawing.CurrentPage();
        var stage = Arena.CurrentStage();

        stage.SetBoundry(page.Width, page.Height, page.Width / 3);

        BoidAreaXY = new Rectangle(0, 0, (int)stage.Width, (int)stage.Height);
        BoidAreaZY = new Rectangle(0, 0, (int)stage.Depth, (int)stage.Height);
        BoidAreaXZ = new Rectangle(0, 0, (int)stage.Width, (int)stage.Depth);


        (Width, Height, Depth) = ((int)stage.Width, (int)stage.Height, (int)stage.Depth);
    }

    public Tuple<FoShape2D, FoShape3D> EstablishShape(Boid boid, FoPage2D page, FoStage3D stage, int size = 40)
    {
        var shape2D = new FoShape2D(size, size, boid.Color);
        var shape3D = new FoShape3D(boid.BoidId, boid.Color);
        shape3D.CreateBox(boid.BoidId, .1, .1, .1);

        // var url = $"{Workspace.GetBaseUrl()}storage/StaticFiles/3DModels/jet.glb";
        // url.WriteSuccess();
        // shape3D.CreateGlb(url, .1, .1, .1);

        page.AddShape(shape2D);
        stage.AddShape(shape3D);
        ApplyExternalMethods(boid, shape2D, shape3D);

        // Arena.PreRender(shape3D);
        return Tuple.Create(shape2D, shape3D);
    }

    public void CreateShapesForBoids(List<Boid> boids)
    {
        var page = Drawing.CurrentPage();
        var stage = Arena.CurrentStage();
        foreach (var boid in boids)
        {
            EstablishShape(boid, page, stage);
        }

    }


    public void DoLoad3dModel(string filename)
    {
        var arena = Workspace.GetArena();
        if (arena == null) return;

        // var baseURL = Path.Join(Workspace.GetBaseUrl(), "storage", "StaticFiles");
        var baseURL = $"{Workspace.GetBaseUrl()}storage/StaticFiles";
        baseURL.WriteSuccess();
        arena.Load3DModelFromFile("3DModels", filename, baseURL);
    }


    public void CreateBoids(int boidCount, string color)
    {
        for (int i = 0; i < boidCount; i++)
        {
            var boid = new Boid(Rand, Width, Height, Depth, color);
            LocalBoids.Add(boid);
        }
    }


    public void BoidModelCreate(D2D_ModelCreate model)
    {
        "Call to create model".WriteInfo();
        var obj = StorageHelpers.HydrateObject(typeof(Boid), model.Payload);
        if (obj is Boid boid)
        {
            ForeignBoids.Add(boid.BoidId, boid);

            var page = Drawing.CurrentPage();
            var stage = Arena.CurrentStage();
            if (FieldShape == null)
            {
                var item = EstablishShape(boid, page, stage, 20);
                item.Item1.Color = item.Item2.Color = "Black";
            }

            IsRunning = true;
        }

    }


    public void BoidModelUpdate(D2D_ModelUpdate model)
    {
        //  "Call to update model".WriteInfo();
        if (ForeignBoids.TryGetValue(model.TargetId, out Boid? boid) == true)
        {
            boid.MoveXY(model.PinX, model.PinY, model.Angle);
        }

    }
    public void BoidModelReflect(D2D_ModelUpdate model)
    {
        //"Call to BoidModelReflect".WriteInfo();
        if (ReflectedBoids.TryGetValue(model.TargetId, out Boid? boid) == true)
        {
            boid.MoveXY(model.PinX, model.PinY, model.Angle);
            return;
        }

        //create shadow boids 

        var obj = StorageHelpers.HydrateObject(typeof(Boid), model.Payload);
        if (obj is Boid newboid)
        {
            ReflectedBoids.Add(newboid.BoidId, newboid);
            newboid.MoveXY(model.PinX, model.PinY, model.Angle);

            var page = Workspace.GetDrawing().CurrentPage();
            var stage = Workspace.GetArena().CurrentStage();
            if (FieldShape == null)
            {
                var item = EstablishShape(newboid, page, stage, 20);
                item.Item1.Color = item.Item2.Color = "Black";
            }
        }

    }

    public void BoidModelDestroy(D2D_ModelDestroy model)
    {
        "Call to destroy model".WriteInfo();
        var page = Drawing.CurrentPage();

        ForeignBoids.Remove(model.TargetId);
        page.ExtractShapes(model.TargetId);
        page.ExtractWhere<FoShape2D>(item => item.GlyphId == model.TargetId);
    }



    protected void SendModelCreated<T>(T model) where T : Boid
    {
        if ( !Command.IsConnected ) return;

        var create = new D2D_ModelCreate()
        {
            PayloadType = model.GetType().Name,
            Payload = StorageHelpers.Dehydrate<T>(model, false)
        };

        Command.SendSyncMessage(create);
    }

    protected void SendModelMoved<T>(T model) where T : Boid
    {
        if (!Command.IsConnected) return;

        var move = new D2D_ModelUpdate(model.BoidId, model)
        {
            PinX = (int)model.X,
            PinY = (int)model.Y,
            Angle = model.AngleXY
        };
        Command.SendSyncMessage(move);
    }

    protected void SendModelDestroy<T>(T model) where T : Boid
    {
        if (!Command.IsConnected) return;

        var destroy = new D2D_ModelDestroy()
        {
            TargetId = model.BoidId,
            PayloadType = model.GetType().Name
        };
        Command.SendSyncMessage(destroy);
    }

    public void BoidsSub5()
    {
        var removed = AdjustBoidCountBy(-5);
        var ids = removed.Select(item => item.BoidId).ToList();

        removed.ForEach(boid => SendModelDestroy<Boid>(boid));

        IsRunning = ids.Count > 0;

        var page = Drawing.CurrentPage();
        page.ExtractWhere<FoShape2D>(item => ids.Contains(item.GlyphId));
        var stage = Arena.CurrentStage();
        stage.ExtractWhere<FoShape3D>(item => ids.Contains(item.GlyphId));
    }

    public void BoidsAdd5()
    {
        var added = AdjustBoidCountBy(5);
        added.ForEach(boid => SendModelCreated<Boid>(boid));

        IsRunning = true;

        //only create local shape
        if (FieldShape != null) return;

        CreateShapesForBoids(added);
    }

    public void BoidsAdd25()
    {
        var added = AdjustBoidCountBy(25);
        added.ForEach(boid => SendModelCreated<Boid>(boid));

        IsRunning = true;

        //only create local shape
        if (FieldShape != null) return;

        CreateShapesForBoids(added);
    }
    public void BoidsAdd100()
    {
        var added = AdjustBoidCountBy(100);
        added.ForEach(boid => SendModelCreated<Boid>(boid));

        IsRunning = true;

        //only create local shape
        if (FieldShape != null) return;

        CreateShapesForBoids(added);
    }
    public void RunBoids()
    {
        IsRunning = true;
    }

    public void StopBoids()
    {
        IsRunning = false;
    }

    //  https://swharden.com/blog/2021-01-08-blazor-boids/

    public void ToggleFieldShape()
    {
        IsRunning = false;
        if (FieldShape == null)
        {
            var page = Drawing.CurrentPage();
            page.ClearAll();

            var BoidCount = 10;
            CreateBoids(BoidCount, RandomColor());
            LocalBoids.ForEach(boid => SendModelCreated<Boid>(boid));
            IsRunning = true;

            FieldShape = new FoFieldShape2D("Boids", (int)Width, (int)Height, "Pink")
            {
                DrawSimulation = async (ctx) =>
                {
                    foreach (var boid in LocalBoids)
                        await DrawABoid(ctx, boid, 40);
                    foreach (var boid in ForeignBoids.Values)
                        await DrawABoid(ctx, boid, 20);
                }
            };

            Drawing.AddShape<FoFieldShape2D>(FieldShape);
        }
        else
        {

            FieldShape = null;
            ToggleBoids();
        }
    }

    private static async Task DrawABoid(Canvas2DContext ctx, Boid boid, int size)
    {
        var x = boid.X;
        var y = boid.Y;
        var a = boid.AngleXY * Matrix2D.DEG_TO_RAD;

        await ctx.SaveAsync();
        await ctx.TranslateAsync(x, y);
        await ctx.RotateAsync((float)a);
        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(size, 0);
        await ctx.LineToAsync(0, 0);
        await ctx.LineToAsync(size / 2, size);
        await ctx.LineToAsync(size, 0);

        await ctx.ClosePathAsync();
        await ctx.FillAsync();
        await ctx.RestoreAsync();
    }

    public void ToggleBoids()
    {

        if (!IsRunning && LocalBoids.Count == 0)
        {
            var BoidCount = 10;
            CreateBoids(BoidCount, RandomColor());
            LocalBoids.ForEach(boid => SendModelCreated<Boid>(boid));
            CreateShapesForBoids(LocalBoids);
        }

        IsRunning = !IsRunning;
    }
    public List<Boid> AdjustBoidCountBy(int count)
    {
        var change = new List<Boid>();

        if (count < 0)
        {
            var extract = Math.Abs(count);
            extract = extract < LocalBoids.Count ? extract : LocalBoids.Count;
            for (int i = 0; i < extract; i++)
            {
                change.Add(LocalBoids[0]);
                LocalBoids.RemoveAt(0);
            }
        }
        else
        {
            var color = RandomColor();
            for (int i = 0; i < count; i++)
            {
                var boid = new Boid(Rand, Width, Height, Depth, color);
                change.Add(boid);
            }
            LocalBoids.AddRange(change);
        }
        return change;
    }



    public static void ApplyExternalMethods(Boid boid, FoGlyph2D shape2D, FoGlyph3D shape3D)
    {
        shape2D.GlyphId = boid.BoidId;
        
        shape2D.ShapeDraw = async (ctx, obj) =>
        {
            var w = shape2D.Width;
            var h = shape2D.Height;
            await ctx.SaveAsync();

            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(w / 2, h);
            await ctx.LineToAsync(w, 0);
            await ctx.LineToAsync(0, 0);
            await ctx.LineToAsync(w / 2, h);

            await ctx.ClosePathAsync();
            await ctx.FillAsync();
            await ctx.RestoreAsync();
        };


        shape2D.ContextLink = (obj, tick) =>
        {
            obj.PinX = (int)boid.X;
            obj.PinY = (int)boid.Y;
            obj.Angle = boid.AngleXY;

            //need the code to map page dimensions to arean dimensions
            var x = boid.X * 0.01;
            var y = boid.Z * 0.01;
            var z = boid.Y * 0.01;
            shape3D.UpdateMeshPosition(x, y, z);
        };
    }


    public async Task RenderWatermark(Canvas2DContext ctx, int tick)
    {
        if (FieldShape == null)
        {
            await ctx.BeginPathAsync();
            await ctx.SetLineDashAsync(new float[] { 10, 10 });
            await ctx.SetLineWidthAsync(3);
            await ctx.SetStrokeStyleAsync("Red");
            var rect = Drawing.TransformRect(BoidAreaXY);
            await ctx.StrokeRectAsync(rect.X, rect.Y, rect.Width, rect.Height);
            var x = rect.Width + rect.X + 50;
            var y = rect.Height + rect.Y + 50;

            // //now draw to the right
            // rect = Drawing.TransformRect(BoidAreaZY);
            // await ctx.StrokeRectAsync(x + rect.X, rect.Y, rect.Width, rect.Height);
            // //now draw to the bottom
            // rect = Drawing.TransformRect(BoidAreaXZ);
            // await ctx.StrokeRectAsync(rect.X, y + rect.Y, rect.Width, rect.Height);

            await ctx.StrokeAsync();
        }
        //this is necessary to make sure the boids move in 3D
        await Arena.UpdateArena();

    }


    public static string RandomColor()
    {
        Random rnd = new();
        var colors = new List<string> { "White", "Indigo", "SaddleBrown", "Salmon", "Red", "Purple", "LightGreen", "SteelBlue", "Red", "Black" };
        var index = rnd.Next(colors.Count);
        return colors[index];
    }

    public void Resize(double width, double height) => (Width, Height) = (width, height);
    public void Resize(double width, double height, double depth) => (Width, Height, Depth) = (width, height, depth);


    public List<Boid> Advance(bool bounceOffWalls = true, bool wrapAroundEdges = false)
    {
        if (!IsRunning) return LocalBoids;

        // update void speed and direction (velocity) based on rules
        foreach (var boid in LocalBoids)
        {
            (double flockXvel, double flockYvel, double flockZvel) = Flock(boid, 50, .0003);
            (double alignXvel, double alignYvel, double alignZvel) = Align(boid, 50, .01);
            (double avoidXvel, double avoidYvel, double avoidZvel) = Avoid(boid, 20, .001);
            (double predXvel, double predYval, double predZval) = Predator(boid, 150, .00005);
            boid.Xvel += flockXvel + avoidXvel + alignXvel + predXvel;
            boid.Yvel += flockYvel + avoidYvel + alignYvel + predYval;
            boid.Zvel += flockZvel + avoidZvel + alignZvel + predZval;
        }

        // move all boids forward in time
        foreach (var boid in LocalBoids)
        {
            boid.MoveForward();
            if (bounceOffWalls)
                BounceOffWalls(boid);
            if (wrapAroundEdges)
                WrapAround(boid);

            boid.AngleXY = boid.GetAngleXY();
            SendModelMoved<Boid>(boid);
        }


        return LocalBoids;
    }

    private (double xVel, double yVel, double zVel) Flock(Boid boid, double distance, double power)
    {
        // point toward the center of the flock (mean flock boid position)
        var neighbors = LocalBoids.Where(x => x.GetDistance(boid) < distance);
        double meanX = neighbors.Sum(x => x.X) / neighbors.Count();
        double meanY = neighbors.Sum(x => x.Y) / neighbors.Count();
        double meanZ = neighbors.Sum(x => x.Z) / neighbors.Count();
        double deltaCenterX = meanX - boid.X;
        double deltaCenterY = meanY - boid.Y;
        double deltaCenterZ = meanZ - boid.Z;
        return (deltaCenterX * power, deltaCenterY * power, deltaCenterZ * power);
    }

    private (double xVel, double yVel, double zVel) Avoid(Boid boid, double distance, double power)
    {
        // point away as boids get close
        var neighbors = LocalBoids.Where(x => x.GetDistance(boid) < distance);
        (double sumClosenessX, double sumClosenessY, double sumClosenessZ) = (0, 0, 0);
        foreach (var neighbor in neighbors)
        {
            double closeness = distance - boid.GetDistance(neighbor);
            sumClosenessX += (boid.X - neighbor.X) * closeness;
            sumClosenessY += (boid.Y - neighbor.Y) * closeness;
            sumClosenessZ += (boid.Z - neighbor.Z) * closeness;
        }
        return (sumClosenessX * power, sumClosenessY * power, sumClosenessZ * power);
    }

    private (double xVel, double yVel, double zVel) Predator(Boid boid, double distance, double power)
    {
        // point away as predators get close
        (double sumClosenessX, double sumClosenessY, double sumClosenessZ) = (0, 0, 0);
        for (int i = 0; i < PredatorCount; i++)
        {
            Boid predator = LocalBoids[i];
            double distanceAway = boid.GetDistance(predator);
            if (distanceAway < distance)
            {
                double closeness = distance - distanceAway;
                sumClosenessX += (boid.X - predator.X) * closeness;
                sumClosenessY += (boid.Y - predator.Y) * closeness;
                sumClosenessZ += (boid.Z - predator.Z) * closeness;
            }
        }
        return (sumClosenessX * power, sumClosenessY * power, sumClosenessZ * power);
    }

    private (double xVel, double yVel, double zVel) Align(Boid boid, double distance, double power)
    {
        // point toward the center of the flock (mean flock boid position)
        var neighbors = LocalBoids.Where(x => x.GetDistance(boid) < distance);
        double meanXvel = neighbors.Sum(x => x.Xvel) / neighbors.Count();
        double meanYvel = neighbors.Sum(x => x.Yvel) / neighbors.Count();
        double meanZvel = neighbors.Sum(x => x.Zvel) / neighbors.Count();
        double dXvel = meanXvel - boid.Xvel;
        double dYvel = meanYvel - boid.Yvel;
        double dZvel = meanZvel - boid.Zvel;
        return (dXvel * power, dYvel * power, dZvel * power);
    }

    private void BounceOffWalls(Boid boid)
    {
        double pad = 50;
        double turn = .5;
        if (boid.X < pad)
            boid.Xvel += turn;
        if (boid.X > Width - pad)
            boid.Xvel -= turn;

        if (boid.Y < pad)
            boid.Yvel += turn;
        if (boid.Y > Height - pad)
            boid.Yvel -= turn;

        if (boid.Z < pad)
            boid.Zvel += turn;
        if (boid.Z > Depth - pad)
            boid.Zvel -= turn;
    }

    private void WrapAround(Boid boid)
    {
        if (boid.X < 0)
            boid.X += Width;
        if (boid.X > Width)
            boid.X -= Width;

        if (boid.Y < 0)
            boid.Y += Height;
        if (boid.Y > Height)
            boid.Y -= Height;

        if (boid.Z < 0)
            boid.Z += Depth;
        if (boid.Z > Depth)
            boid.Z -= Depth;

    }


}

