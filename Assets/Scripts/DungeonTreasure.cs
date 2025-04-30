using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class DungeonTreasure : MonoBehaviour
{
    private Transform _lid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lid = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async UniTask Open(CancellationToken token)
    {
        var offset = _lid.position;

        try
        {
            _lid.position += new Vector3(0, 0.1f, 0);
            await UniTask.WaitForSeconds(1, cancellationToken: token);
            _lid.position += new Vector3(0, 0.1f, 0);
            await UniTask.WaitForSeconds(1, cancellationToken: token);
            _lid.position += new Vector3(0, 0.1f, 0);
            await UniTask.WaitForSeconds(1, cancellationToken: token);
            _lid.gameObject.SetActive(false);
            await UniTask.WaitForSeconds(1, cancellationToken: token);
        }
        finally
        {
            _lid.gameObject.SetActive(true);
            _lid.position = offset;
        }
    }
}
