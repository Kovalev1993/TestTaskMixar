using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(TouchResolver))]
public class CubesBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private float _deleteTouchDuration;
    [SerializeField] private GameObject _controls;
    [SerializeField] private ImagesTracker _imagesTracker;

    private ARPlaneManager _planeManager;
    private TouchResolver _raycastResolver;
    private Coroutine _deletingCoroutine;
    private List<InstallableCube> _cubes = new();

    public void StartMoveCubesRight()
    {
        foreach (var cube in _cubes)
        {
            cube.StartMoveRight();
        }
    }

    public void StartMoveCubesLeft()
    {
        foreach (var cube in _cubes)
        {
            cube.StartMoveLeft();
        }
    }

    public void StopMoveCubes()
    {
        foreach (var cube in _cubes)
        {
            cube.StopMove();
        }
    }

    private void Awake()
    {
        _raycastResolver = GetComponent<TouchResolver>();
        _planeManager = GetComponent<ARPlaneManager>();
    }

    private void Start()
    {
        _raycastResolver.GetPlaneDetectionEvent().AddListener(InstantiateCube);
        _raycastResolver.GetObjectDetectionEvent().AddListener(OnCubeClick);
        _raycastResolver.GetTouchEventEvent().AddListener(StopDeletingCoroutine);
        _planeManager.planesChanged += UpdateCubes;
    }

    private void InstantiateCube(ARRaycastHit hitInfo)
    {
        _cubes.Add(
            Instantiate(_cubePrefab, hitInfo.pose.position, Quaternion.identity).GetComponent<InstallableCube>()
        );
        _cubes[_cubes.Count - 1].SetImageTracker(_imagesTracker);
        _controls.SetActive(true);
    }

    private void OnCubeClick(RaycastHit hitInfo)
    {
        if (hitInfo.collider.TryGetComponent<InstallableCube>(out var cube))
        {
            cube.ChangeColor();
            _deletingCoroutine = StartCoroutine(ScheduleCubeDestroying(cube.gameObject));
        }
    }

    private IEnumerator ScheduleCubeDestroying(GameObject cube)
    {
        yield return new WaitForSeconds(_deleteTouchDuration);
        _cubes.Remove(cube.GetComponent<InstallableCube>());
        Destroy(cube);
        if(_cubes.Count == 0)
            _controls.SetActive(false);
    }

    private void StopDeletingCoroutine()
    {
        if(_deletingCoroutine != null)
            StopCoroutine(_deletingCoroutine);
    }

    private void UpdateCubes(ARPlanesChangedEventArgs changes)
    {
        if (0 < changes.updated.Count)
        {
            var plane = changes.updated[0];
            foreach (var cube in _cubes)
            {
                cube.transform.position = new Vector3(cube.transform.position.x, plane.transform.position.y, cube.transform.position.z);
            }
        }
    }

    private void OnDestroy()
    {
        _raycastResolver.GetPlaneDetectionEvent().RemoveListener(InstantiateCube);
        _raycastResolver.GetObjectDetectionEvent().RemoveListener(OnCubeClick);
        _raycastResolver.GetTouchEventEvent().RemoveListener(StopDeletingCoroutine);
        _planeManager.planesChanged -= UpdateCubes;
    }
}
