// Assets/Scripts/Core/InputHandler.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _boardMask;
    [SerializeField] private GameManager _gm;
    [SerializeField] private StoneController _stone;

    [Header("Touch Preview")]
    [SerializeField] private float _previewRadius = 0.42f;
    [SerializeField] private float _previewYOffset = 0.12f;
    [SerializeField, Range(0f, 1f)] private float _previewAlpha = 0.55f;

    private bool _enabled;
    private bool _waitingForPointerRelease;
    private int _enabledFrame = -1;
    private GameObject _preview;
    private MeshRenderer _previewRenderer;
    private Material _previewMaterial;

    private void OnDisable()
    {
        HidePreview();
    }

    private void OnDestroy()
    {
        if (_preview != null)
            Destroy(_preview);
        if (_previewMaterial != null)
            Destroy(_previewMaterial);
    }

    public void SetEnabled(bool on)
    {
        if (!on)
        {
            _enabled = false;
            _waitingForPointerRelease = false;
            HidePreview();
            Debug.Log($"[InputHandler] SetEnabled({on})");
            return;
        }

        _enabled = on;
        _enabledFrame = Time.frameCount;
        _waitingForPointerRelease = HasActivePointer();
        Debug.Log($"[InputHandler] SetEnabled({on})");
    }

    private void Update()
    {
        if (!_enabled)
        {
            HidePreview();
            return;
        }

        if (Time.frameCount <= _enabledFrame)
        {
            HidePreview();
            return;
        }

        if (_waitingForPointerRelease)
        {
            HidePreview();
            if (HasActivePointer()) return;
            _waitingForPointerRelease = false;
        }

        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            HandleTouch(touch);
            return;
        }

        if (!Application.isMobilePlatform)
        {
            HandleMouse();
            return;
        }

        HidePreview();
    }

    private void HandleTouch(Touch touch)
    {
        if (touch.phase == TouchPhase.Canceled)
        {
            HidePreview();
            return;
        }

        if (IsPointerOverUI(touch.fingerId))
        {
            HidePreview();
            return;
        }

        if (touch.phase == TouchPhase.Ended)
        {
            TryPlaceAtScreenPosition(touch.position);
            HidePreview();
            return;
        }

        UpdatePreview(touch.position);
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButton(0))
        {
            if (IsPointerOverUI())
            {
                HidePreview();
                return;
            }

            UpdatePreview(Input.mousePosition);
            return;
        }

        if (Input.GetMouseButtonUp(0) && !IsPointerOverUI())
            TryPlaceAtScreenPosition(Input.mousePosition);

        HidePreview();
    }

    private void TryPlaceAtScreenPosition(Vector2 screen)
    {
        if (!TryGetPlayableCell(screen, out int row, out int col, out _))
            return;

        _gm.OnBoardTapped(row, col);
    }

    private void UpdatePreview(Vector2 screen)
    {
        if (!TryGetPlayableCell(screen, out _, out _, out var world))
        {
            HidePreview();
            return;
        }

        EnsurePreview();
        _preview.transform.position = new Vector3(world.x, world.y + _previewYOffset, world.z);
        SetPreviewColor(_gm.Turn.Current);

        if (!_preview.activeSelf)
            _preview.SetActive(true);
    }

    private bool TryGetPlayableCell(Vector2 screen, out int row, out int col, out Vector3 world)
    {
        row = -1;
        col = -1;
        world = Vector3.zero;

        var ray = _cam.ScreenPointToRay(screen);
        if (!Physics.Raycast(ray, out var hit, 100f, _boardMask))
            return false;

        (row, col) = _stone.WorldToGrid(hit.point);

        if (row < 0 || row >= BoardManager.Size || col < 0 || col >= BoardManager.Size)
            return false;

        if (_gm.Board.Board[row, col] != 0)
            return false;

        world = _stone.GridToWorld(row, col);
        return true;
    }

    private void EnsurePreview()
    {
        if (_preview != null) return;

        _preview = new GameObject("TouchPlacementPreview");
        _preview.transform.SetParent(transform, true);

        var meshFilter = _preview.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateDiskMesh(_previewRadius, 32);

        _previewRenderer = _preview.AddComponent<MeshRenderer>();
        _previewRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _previewRenderer.receiveShadows = false;

        _previewMaterial = CreatePreviewMaterial();
        _previewRenderer.material = _previewMaterial;
        _preview.SetActive(false);
    }

    private void HidePreview()
    {
        if (_preview != null && _preview.activeSelf)
            _preview.SetActive(false);
    }

    private void SetPreviewColor(Player player)
    {
        if (_previewMaterial == null) return;

        var color = player == Player.Black
            ? new Color(0.02f, 0.02f, 0.02f, _previewAlpha)
            : new Color(1f, 0.98f, 0.9f, _previewAlpha);

        if (_previewMaterial.HasProperty("_BaseColor"))
            _previewMaterial.SetColor("_BaseColor", color);
        if (_previewMaterial.HasProperty("_Color"))
            _previewMaterial.SetColor("_Color", color);
    }

    private static Material CreatePreviewMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        var material = new Material(shader);
        ConfigureTransparentMaterial(material);
        return material;
    }

    private static void ConfigureTransparentMaterial(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        if (material.HasProperty("_Surface"))
            material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Blend"))
            material.SetFloat("_Blend", 0f);
        if (material.HasProperty("_SrcBlend"))
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (material.HasProperty("_DstBlend"))
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (material.HasProperty("_ZWrite"))
            material.SetFloat("_ZWrite", 0f);

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
    }

    private static Mesh CreateDiskMesh(float radius, int segments)
    {
        int vertexCount = segments + 1;
        var vertices = new Vector3[vertexCount];
        var normals = new Vector3[vertexCount];
        var triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        normals[0] = Vector3.up;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.PI * 2f * i / segments;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            normals[i + 1] = Vector3.up;
        }

        for (int i = 0; i < segments; i++)
        {
            int tri = i * 3;
            int current = i + 1;
            int next = i == segments - 1 ? 1 : i + 2;

            triangles[tri] = 0;
            triangles[tri + 1] = next;
            triangles[tri + 2] = current;
        }

        var mesh = new Mesh { name = "TouchPlacementPreviewMesh" };
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }

    private static bool HasActivePointer()
    {
        return Input.touchCount > 0 || Input.GetMouseButton(0);
    }

    private static bool IsPointerOverUI(int pointerId = -1)
    {
        if (EventSystem.current == null) return false;

        return pointerId >= 0
            ? EventSystem.current.IsPointerOverGameObject(pointerId)
            : EventSystem.current.IsPointerOverGameObject();
    }
}
