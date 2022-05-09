using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using VoxelSystem;

[ExecuteAlways] //to execute in the editor
public class VoxelsRendererDynamic : MonoBehaviour
{
    private Vector4 _voxelColor;
    private Texture2D _texPosition;
    private Texture2D _texColor;
    private VisualEffect vfx;
    private uint _resolution = 2048;
    private bool _toUpdate;
    private uint _particleCount;
    private float _voxelSize;
    private bool hasColor;

    private void Start()
    {
        vfx = GetComponent<VisualEffect>();
    }
    /*
    /// <summary>
    /// Visualise voxel model having one color value
    /// </summary>
    public void UpdateVoxelVisualisation(bool[,,] voxels, Bounds bounds, float voxelSize, int filledVoxels, Color color)
    {        
        positions = new Vector3[filledVoxels];
        colors = new Color[filledVoxels];
        int sizeX = voxels.GetLength(0);
        int sizeY = voxels.GetLength(1);
        int sizeZ = voxels.GetLength(2);

        Vector3 initModelPos = new Vector3(bounds.min.x + voxelSize / 2, bounds.min.y + voxelSize / 2, bounds.min.z + voxelSize / 2);

        int m = 0;
        for (int i = 0; i < sizeX; i++)
            for (int j = 0; j < sizeY; j++)
                for (int k = 0; k < sizeZ; k++)
                {
                    var currentVoxel = voxels[i, j, k];
                    if (currentVoxel)
                    {
                        Vector3 voxelPosition = new Vector3(initModelPos.x + i * voxelSize, initModelPos.y + j * voxelSize, initModelPos.z + k * voxelSize);
                        positions[m] = voxelPosition;
                        colors[m] = color;
                        m++;
                    }
                }

        SetParticles(positions, colors);
    }

    /// <summary>
    /// Visualise voxel model with many components
    /// </summary>
    public void UpdateVoxelVisualisation(int[,,] voxels, Bounds bounds, float voxelSize, int filledVoxels, int[] classes)
    {
        positions = new Vector3[filledVoxels];
        colors = new Color[filledVoxels];
        int sizeX = voxels.GetLength(0);
        int sizeY = voxels.GetLength(1);
        int sizeZ = voxels.GetLength(2);

        Vector3 initModelPos = new Vector3(bounds.min.x + voxelSize / 2, bounds.min.y + voxelSize / 2, bounds.min.z + voxelSize / 2);
        Dictionary<int, Color> uniqueColors = VisFunctions.GetUniqueColors(classes);

        int m = 0;
        for (int i = 0; i < sizeX; i++)
            for (int j = 0; j < sizeY; j++)
                for (int k = 0; k < sizeZ; k++)
                {
                    var currentVoxel = voxels[i, j, k];
                    if (currentVoxel != 0)
                    {
                        Vector3 voxelPosition = new Vector3(initModelPos.x + i * voxelSize, initModelPos.y + j * voxelSize, initModelPos.z + k * voxelSize);
                        positions[m] = voxelPosition;
                        colors[m] = uniqueColors[currentVoxel];
                        m++;
                    }
                }

        SetParticles(positions, colors);
    }

    public void UpdateVoxelVisualisation(int[,,] voxels, Bounds bounds, float voxelSize, int filledVoxels, int[] classes, Dictionary<int, Color> uniColors)
    {
        positions = new Vector3[filledVoxels];
        colors = new Color[filledVoxels];
        int sizeX = voxels.GetLength(0);
        int sizeY = voxels.GetLength(1);
        int sizeZ = voxels.GetLength(2);

        Vector3 initModelPos = new Vector3(bounds.min.x + voxelSize / 2, bounds.min.y + voxelSize / 2, bounds.min.z + voxelSize / 2);

        int m = 0;
        for (int i = 0; i < sizeX; i++)
            for (int j = 0; j < sizeY; j++)
                for (int k = 0; k < sizeZ; k++)
                {
                    var currentVoxel = voxels[i, j, k];
                    if (currentVoxel != 0)
                    {
                        Vector3 voxelPosition = new Vector3(initModelPos.x + i * voxelSize, initModelPos.y + j * voxelSize, initModelPos.z + k * voxelSize);
                        positions[m] = voxelPosition;
                        colors[m] = uniColors[currentVoxel];
                        m++;
                    }
                }

        SetParticles(positions, colors);
    }

    public void UpdateVoxelVisualisation(List<Vector3> voxelVectors, float voxelSize, Color uniqueColor)
    {
        int count = voxelVectors.Count;
        positions = new Vector3[count+10000];
        colors = new Color[count+10000];

        int m = 0;
        foreach (var voxelVector in voxelVectors)
        {
            Vector3 voxelPosition = voxelVector;
            positions[m] = voxelPosition;
            colors[m] = uniqueColor;
            m++;
        }

        SetParticles(positions, colors);
    }

    public void UpdateVoxelVisualisation(List<int>[,,] voxels, Bounds bounds, float voxelSize, int filledVoxels, int[] classes)
    {
        positions = new Vector3[filledVoxels];
        colors = new Color[filledVoxels];
        int sizeX = voxels.GetLength(0);
        int sizeY = voxels.GetLength(1);
        int sizeZ = voxels.GetLength(2);

        Vector3 initModelPos = new Vector3(bounds.min.x + voxelSize / 2, bounds.min.y + voxelSize / 2, bounds.min.z + voxelSize / 2);
        Dictionary<int, Color> uniqueColors = VisFunctions.GetUniqueColors(classes);

        int m = 0;
        for (int i = 0; i < sizeX; i++)
            for (int j = 0; j < sizeY; j++)
                for (int k = 0; k < sizeZ; k++)
                {
                    var currentVoxel = voxels[i, j, k];
                    if (currentVoxel != null)
                    {
                        Vector3 voxelPosition = new Vector3(initModelPos.x + i * voxelSize, initModelPos.y + j * voxelSize, initModelPos.z + k * voxelSize);
                        positions[m] = voxelPosition;
                        int count = currentVoxel.Count();
                        if (count == 1)
                        {
                            colors[m] = uniqueColors[currentVoxel[0]];
                        }
                        else
                        {
                            colors[m] = Color.white;
                        }
                        m++;
                    }
                }

        SetParticles(positions, colors);
    }

    public void SetParticles(Vector3[] positions, Color[] colors)
    {
        texColor = new Texture2D(positions.Length > (int)resolution ? (int)resolution : positions.Length, Mathf.Clamp(positions.Length / (int)resolution, 1, (int)resolution), TextureFormat.RGBAFloat, false);
        texPosScale = new Texture2D(positions.Length > (int)resolution ? (int)resolution : positions.Length, Mathf.Clamp(positions.Length / (int)resolution, 1, (int)resolution), TextureFormat.RGBAFloat, false);
        int texWidth = texColor.width;
        int texHeight = texColor.height;

        for (int y = 0; y < texHeight; y++)
        {
            for (int x = 0; x < texWidth; x++)
            {
                int index = x + y * texWidth;
                texColor.SetPixel(x, y, colors[index]);
                var data = new Color(positions[index].x, positions[index].y, positions[index].z, voxelSize);
                texPosScale.SetPixel(x, y, data);
            }
        }

        texColor.Apply();
        texPosScale.Apply();
        particleCount = (uint)positions.Length;
        toUpdate = true;
    }
    */


    public void SetParticles(List<Color> positions)
    {
        _texPosition = new Texture2D(_particleCount > _resolution ? (int)_resolution : (int)_particleCount, Mathf.Clamp((int)_particleCount / (int)_resolution, 1, (int)_resolution), TextureFormat.RGBAFloat, false);

        var y = 0;
        var x = 0;
        foreach (var position in positions)
        {
            _texPosition.SetPixel(x, y, position);

            if (x < _resolution)
            {
                x++;
            }
            else
            {
                x = 0;
                y++;
            }
        }

        _texPosition.Apply();
        _toUpdate = true;
    }

    public void SetParticlesWithColor(List<Color> positions, List<Color> colors)
    {
        _texPosition = new Texture2D(_particleCount > _resolution ? (int)_resolution : (int)_particleCount, Mathf.Clamp((int)_particleCount / (int)_resolution, 1, (int)_resolution), TextureFormat.RGBAFloat, false);
        _texColor = new Texture2D(_particleCount > _resolution ? (int)_resolution : (int)_particleCount, Mathf.Clamp((int)_particleCount / (int)_resolution, 1, (int)_resolution), TextureFormat.RGBAFloat, false);

        var y = 0;
        var x = 0;
        var i = 0;
        foreach (var position in positions)
        {
            _texPosition.SetPixel(x, y, position);
            _texColor.SetPixel(x,y,colors[i]);

            if (x < _resolution)
            {
                x++;
            }
            else
            {
                x = 0;
                y++;
            }

            i++;
        }

        _texPosition.Apply();
        _texColor.Apply();
        _toUpdate = true;
    }

    public void SetVoxelParticles(Voxel_t[] voxels, int width, int height, Vector3 pivotPoint, float voxelSize, Color voxelsColor)
    {
        var voxelsCount = voxels.Count(x => x.fill > 0);
        _texPosition = new Texture2D(voxelsCount > (int)_resolution ? (int)_resolution : voxelsCount, Mathf.Clamp(voxelsCount / (int)_resolution, 1, (int)_resolution), TextureFormat.RGBAFloat, false);
        _voxelSize = voxelSize;

        var y = 0;
        var x = 0;
        for (var i = 0; i < voxels.Length; i++)
        {
            var voxel = voxels[i];

            if (voxel.fill <= 0) continue;
            
            var voxel3DIndex = ArrayFunctions.Index1DTo3D(i, width, height);

            var position = new Color(voxel3DIndex.X * voxelSize + pivotPoint.x,
                    voxel3DIndex.Y * voxelSize + pivotPoint.y,
                    voxel3DIndex.Z * voxelSize + pivotPoint.z, 0);

            _texPosition.SetPixel(x, y, position);

            if (x < _resolution)
            {
                x++;
            }
            else
            {
                x = 0;
                y++;
            }
        }

        _voxelColor = new Vector4(voxelsColor.r, voxelsColor.g, voxelsColor.b, voxelsColor.a);
        _texPosition.Apply();
        _particleCount = (uint)voxelsCount;
        _toUpdate = true;
    }

    public void SetQuadParticles(Voxel_t[] voxels, int width, int height, int depth, Vector3 pivotPoint, float voxelSize, Color voxelsColor)
    {
        var halfVoxelSize = voxelSize / 2;
        var pivotVoxelPoint = pivotPoint - new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);
        var positions = new List<Color>();

        var quadsCount = 0;
        var m = 0;
        for (int k = 0; k < depth; k++)
            for (int j = 0; j < height; j++)
                for (int i = 0; i < width; i++)
                {
                    var voxel = voxels[m];

                    if (voxel.fill > 0)
                    {
                        var iLeft = i - 1;
                        var iRight = i + 1;
                        var jDown = j - 1;
                        var jUp = j + 1;
                        var kBack = k - 1;
                        var kForward = k + 1;

                        if (iLeft != -1)
                        {
                            var voxelIndex =
                                ArrayFunctions.Index3DTo1D(iLeft, j, k, width, height);
                            var voxelLeft = voxels[voxelIndex];

                            if (voxelLeft.fill == 0)
                            {
                                var position = new Color(pivotVoxelPoint.x + i * voxelSize,
                                    pivotPoint.y + j * voxelSize,
                                    pivotPoint.z + k * voxelSize, 0);
                                positions.Add(position);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(pivotVoxelPoint.x + i * voxelSize,
                                pivotPoint.y + j * voxelSize,
                                pivotPoint.z + k * voxelSize, 0);
                            positions.Add(position);
                            quadsCount++;
                        }

                        if (iRight != width)
                        {
                            var voxelIndex =
                                ArrayFunctions.Index3DTo1D(iRight, j, k, width, height);
                            var voxelRight = voxels[voxelIndex];

                            if (voxelRight.fill == 0)
                            {
                                var position = new Color(pivotVoxelPoint.x + (i + 1) * voxelSize,
                                    pivotPoint.y + j * voxelSize,
                                    pivotPoint.z + k * voxelSize, 1);
                                positions.Add(position);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(pivotVoxelPoint.x + (i + 1) * voxelSize,
                                pivotPoint.y + j * voxelSize,
                                pivotPoint.z + k * voxelSize, 1);
                            positions.Add(position);
                            quadsCount++;
                        }

                        if (jDown != -1)
                        {
                            var voxelIndex =
                                ArrayFunctions.Index3DTo1D(i, jDown, k, width, height);
                            var voxelDown = voxels[voxelIndex];

                            if (voxelDown.fill == 0)
                            {
                                var position = new Color(pivotPoint.x + i * voxelSize,
                                    pivotVoxelPoint.y + j * voxelSize,
                                    pivotPoint.z + k * voxelSize, 2);
                                positions.Add(position);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(pivotPoint.x + i * voxelSize,
                                pivotVoxelPoint.y + j * voxelSize,
                                pivotPoint.z + k * voxelSize, 2);
                            positions.Add(position);
                            quadsCount++;
                        }

                        if (jUp != height)
                        {
                            var voxelIndex = ArrayFunctions.Index3DTo1D(i, jUp, k, width, height);
                            var voxelUp = voxels[voxelIndex];

                            if (voxelUp.fill == 0)
                            {
                                var position = new Color(pivotPoint.x + i * voxelSize,
                                    pivotVoxelPoint.y + (j + 1) * voxelSize,
                                    pivotPoint.z + k * voxelSize, 3);
                                positions.Add(position);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(pivotPoint.x + i * _voxelSize,
                                pivotVoxelPoint.y + (j + 1) * voxelSize,
                                pivotPoint.z + k * voxelSize, 3);
                            positions.Add(position);
                            quadsCount++;
                        }

                        if (kBack != -1)
                        {
                            var voxelIndex =
                                ArrayFunctions.Index3DTo1D(i, j, kBack, width, height);
                            var voxelBack = voxels[voxelIndex];

                            if (voxelBack.fill == 0)
                            {
                                var position = new Color(pivotPoint.x + i * voxelSize,
                                    pivotPoint.y + j * voxelSize,
                                    pivotVoxelPoint.z + k * voxelSize, 4);
                                positions.Add(position);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(pivotPoint.x + i * voxelSize,
                                pivotPoint.y + j * voxelSize,
                                pivotVoxelPoint.z + k * voxelSize, 4);
                            positions.Add(position);
                            quadsCount++;
                        }

                        if (kForward != depth)
                        {
                            var voxelIndex =
                                ArrayFunctions.Index3DTo1D(i, j, kForward, width, height);
                            var voxelForward = voxels[voxelIndex];

                            if (voxelForward.fill == 0)
                            {
                                var position = new Color(pivotPoint.x + i * voxelSize,
                                    pivotPoint.y + j * voxelSize,
                                    pivotVoxelPoint.z + (k + 1) * voxelSize, 5);
                                positions.Add(position);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(pivotPoint.x + i * voxelSize,
                                pivotPoint.y + j * voxelSize,
                                pivotVoxelPoint.z + (k + 1) * voxelSize, 5);
                            positions.Add(position);
                            quadsCount++;
                        }
                    }

                    m++;
                }

        _voxelSize = voxelSize;
        _voxelColor = new Vector4(voxelsColor.r, voxelsColor.g, voxelsColor.b, voxelsColor.a);
        _particleCount = (uint)quadsCount;

        SetParticles(positions);
    }

    public void SetColorQuadParticles(MultiValueVoxelModel voxelModel, float voxelSize)
    {
        hasColor = true;
        var halfVoxelSize = voxelSize / 2;
        var pivotVoxelPoint = voxelModel.PivotPoint - new Vector3(halfVoxelSize, halfVoxelSize, halfVoxelSize);
        var positions = new List<Color>();
        var colors = new List<Color>();

        var quadsCount = 0;
        var m = 0;
        for (int k = 0; k < voxelModel.Depth; k++)
            for (int j = 0; j < voxelModel.Height; j++)
                for (int i = 0; i < voxelModel.Width; i++)
                {
                    var voxel = voxelModel.Voxels[m];

                    if (voxel != null)
                    {
                        var iLeft = i - 1;
                        var iRight = i + 1;
                        var jDown = j - 1;
                        var jUp = j + 1;
                        var kBack = k - 1;
                        var kForward = k + 1;

                        if (iLeft != -1)
                        {
                            var voxelIndex =
                                ArrayFunctions.Index3DTo1D(iLeft, j, k, voxelModel.Width, voxelModel.Height);
                            var voxelLeft = voxelModel.Voxels[voxelIndex];

                            if (voxelLeft == null)
                            {
                                var position = new Color(pivotVoxelPoint.x + i * voxelSize,
                                    voxelModel.PivotPoint.y + j * voxelSize,
                                    voxelModel.PivotPoint.z + k * voxelSize, 0);
                                positions.Add(position);
                                var color = voxel[0].VoxelColor;
                                colors.Add(color);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(pivotVoxelPoint.x + i * voxelSize,
                                voxelModel.PivotPoint.y + j * voxelSize,
                                voxelModel.PivotPoint.z + k * voxelSize, 0);
                            positions.Add(position);
                            var color = voxel[0].VoxelColor;
                            colors.Add(color);
                            quadsCount++;
                        }

                        if (iRight != voxelModel.Width)
                        {
                            var voxelIndex =
                                ArrayFunctions.Index3DTo1D(iRight, j, k, voxelModel.Width, voxelModel.Height);
                            var voxelRight = voxelModel.Voxels[voxelIndex];

                            if (voxelRight == null)
                            {
                                var position = new Color(pivotVoxelPoint.x + (i + 1) * voxelSize,
                                    voxelModel.PivotPoint.y + j * voxelSize,
                                    voxelModel.PivotPoint.z + k * voxelSize, 1);
                                positions.Add(position);
                                var color = voxel[0].VoxelColor;
                                colors.Add(color);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(pivotVoxelPoint.x + (i + 1) * voxelSize,
                                voxelModel.PivotPoint.y + j * voxelSize,
                                voxelModel.PivotPoint.z + k * voxelSize, 1);
                            positions.Add(position);
                            var color = voxel[0].VoxelColor;
                            colors.Add(color);
                            quadsCount++;
                        }

                        if (jDown != -1)
                        {
                            var voxelIndex =
                                ArrayFunctions.Index3DTo1D(i, jDown, k, voxelModel.Width, voxelModel.Height);
                            var voxelDown = voxelModel.Voxels[voxelIndex];

                            if (voxelDown == null)
                            {
                                var position = new Color(voxelModel.PivotPoint.x + i * voxelSize,
                                    pivotVoxelPoint.y + j * voxelSize,
                                    voxelModel.PivotPoint.z + k * voxelSize, 2);
                                positions.Add(position);
                                var color = voxel[0].VoxelColor;
                                colors.Add(color);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(voxelModel.PivotPoint.x + i * voxelSize,
                                pivotVoxelPoint.y + j * voxelSize,
                                voxelModel.PivotPoint.z + k * voxelSize, 2);
                            positions.Add(position);
                            var color = voxel[0].VoxelColor;
                            colors.Add(color);
                            quadsCount++;
                        }

                        if (jUp != voxelModel.Height)
                        {
                            var voxelIndex = ArrayFunctions.Index3DTo1D(i, jUp, k, voxelModel.Width, voxelModel.Height);
                            var voxelUp = voxelModel.Voxels[voxelIndex];

                            if (voxelUp == null)
                            {
                                var position = new Color(voxelModel.PivotPoint.x + i * voxelSize,
                                    pivotVoxelPoint.y + (j + 1) * voxelSize,
                                    voxelModel.PivotPoint.z + k * voxelSize, 3);
                                positions.Add(position);
                                var color = voxel[0].VoxelColor;
                                colors.Add(color);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(voxelModel.PivotPoint.x + i * _voxelSize,
                                pivotVoxelPoint.y + (j + 1) * voxelSize,
                                voxelModel.PivotPoint.z + k * voxelSize, 3);
                            positions.Add(position);
                            var color = voxel[0].VoxelColor;
                            colors.Add(color);
                            quadsCount++;
                        }

                        if (kBack != -1)
                        {
                            var voxelIndex =
                                ArrayFunctions.Index3DTo1D(i, j, kBack, voxelModel.Width, voxelModel.Height);
                            var voxelBack = voxelModel.Voxels[voxelIndex];

                            if (voxelBack == null)
                            {
                                var position = new Color(voxelModel.PivotPoint.x + i * voxelSize,
                                    voxelModel.PivotPoint.y + j * voxelSize,
                                    pivotVoxelPoint.z + k * voxelSize, 4);
                                positions.Add(position);
                                var color = voxel[0].VoxelColor;
                                colors.Add(color);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(voxelModel.PivotPoint.x + i * voxelSize,
                                voxelModel.PivotPoint.y + j * voxelSize,
                                pivotVoxelPoint.z + k * voxelSize, 4);
                            positions.Add(position);
                            var color = voxel[0].VoxelColor;
                            colors.Add(color);
                            quadsCount++;
                        }

                        if (kForward != voxelModel.Depth)
                        {
                            var voxelIndex =
                                ArrayFunctions.Index3DTo1D(i, j, kForward, voxelModel.Width, voxelModel.Height);
                            var voxelForward = voxelModel.Voxels[voxelIndex];

                            if (voxelForward == null)
                            {
                                var position = new Color(voxelModel.PivotPoint.x + i * voxelSize,
                                    voxelModel.PivotPoint.y + j * voxelSize,
                                    pivotVoxelPoint.z + (k + 1) * voxelSize, 5);
                                positions.Add(position);
                                var color = voxel[0].VoxelColor;
                                colors.Add(color);
                                quadsCount++;
                            }
                        }
                        else
                        {
                            var position = new Color(voxelModel.PivotPoint.x + i * voxelSize,
                                voxelModel.PivotPoint.y + j * voxelSize,
                                pivotVoxelPoint.z + (k + 1) * voxelSize, 5);
                            positions.Add(position);
                            var color = voxel[0].VoxelColor;
                            colors.Add(color);
                            quadsCount++;
                        }
                    }

                    m++;
                }

        _voxelSize = voxelSize;
        _particleCount = (uint)quadsCount;

        SetParticlesWithColor(positions, colors);
    }

    private void Update()
    {
        if (_toUpdate)
        {
            _toUpdate = false;

            vfx.Reinit();
            vfx.SetUInt(Shader.PropertyToID("VoxelsCount"), _particleCount);
            vfx.SetFloat(Shader.PropertyToID("VoxelSize"), _voxelSize);
            vfx.SetTexture(Shader.PropertyToID("TexPosScale"), _texPosition);
            vfx.SetUInt(Shader.PropertyToID("Resolution"), _resolution);
            if (hasColor)
            {
                vfx.SetTexture(Shader.PropertyToID("TexColor"), _texColor);
            }
            else
            {
                vfx.SetVector4(Shader.PropertyToID("VoxelColor"), _voxelColor);
            }
        }
    }
}
