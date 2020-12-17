using System.Windows.Media.Media3D;
using System.Linq;
using DynamicData;

namespace TableTopCrucible.Core.Helper
{
    public static class Model3DHelper
    {
        public static void Move(this ModelVisual3D model, Point3D location)
        {
            if (model?.Content?.Bounds == null)
                return;

            var matrix = model.Transform.Value;

            matrix.OffsetX = location.X;
            matrix.OffsetY = location.Y;
            matrix.OffsetZ = location.Z;

            model.Transform = new MatrixTransform3D(matrix);
        }
        public static void Move(this Model3D model, double x, double y, double z) 
            => Move(model, new Point3D(x, y, z));
        public static void Move(this Model3D model, Point3D location)
        {
            if (model?.Bounds == null)
                return;

            var matrix = model.Transform.Value;

            var offset = model.Bounds.GetOriginOffset();

            matrix.OffsetX = location.X - offset.X;
            matrix.OffsetY = location.Y - offset.Y;
            matrix.OffsetZ = location.Z - offset.Z;

            model.Transform = new MatrixTransform3D(matrix);
        }
        public static void Move(this ModelUIElement3D uiElement, Point3D location)
        {
            if (uiElement?.Model?.Bounds == null)
                return;

            var matrix = uiElement.Transform.Value;

            var offset = uiElement.Model.Bounds.GetOriginOffset();

            matrix.OffsetX = location.X - offset.X;
            matrix.OffsetY = location.Y - offset.Y;
            matrix.OffsetZ = location.Z - offset.Z;

            uiElement.Transform = new MatrixTransform3D(matrix);
        }
        public static void PlaceChildrenAtOrigin(this Model3DGroup group)
        {
            foreach (Model3D model in group.Children)
            {
                model.PlaceAtOrigin();
            }
        }
        public static void PlaceAtOrigin(this Model3D model)
            => model.Move(new Point3D(0, 0, 0));

        public static Point3D GetOriginOffset(this Rect3D bounds)
        {
            return new Point3D
            {
                X = bounds.X + bounds.SizeX / 2,
                Y = bounds.Y + bounds.SizeY / 2,
                Z = bounds.Z
            };
        }

        public static void SetMaterial(this Model3DGroup model, Material material)
        {
            if (material == null)
                return;

            model.Children.Where(x => x is Model3DGroup)
                .Cast<Model3DGroup>()
                .ToList()
                .ForEach(x => x.SetMaterial(material));
            model.Children.Where(x => x is GeometryModel3D)
                .Cast<GeometryModel3D>()
                .ToList()
                .ForEach(x =>
                {
                    x.Material = material;
                });
        }
    }
}
