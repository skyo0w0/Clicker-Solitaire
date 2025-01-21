using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Random = UnityEngine.Random;


public class ClickerComponent : MonoBehaviour
{
    internal event Action<int> OnClick;
    
    private int _currentClicks = 0;
    private string _filePath;
    [SerializeField] private SpriteRenderer _spriteRenderer => GetComponent<SpriteRenderer>(); // CanvasGroup для изменения прозрачности
    
    [SerializeField] private float animationDuration = 0.5f; // Длительность анимации
    [SerializeField] private float scaleMultiplier = 1.5f; // Увеличение масштаба при анимации
    [SerializeField] private GameObject particlePrefab; // Префаб огонька
    [SerializeField] private int particleCount = 10; // Количество огоньков
    [SerializeField] private float _particleAnimationDuration = 1f; // Длительность анимации огоньков
    [SerializeField] private float spreadRadius = 1f; // Радиус разброса огоньков
    

    
    private Vector3 _originalScale;
    private float _originalAlpha ;
    
    #region Unity Life Cycle
    private void Start()
    {
        _filePath = Path.Combine(Application.persistentDataPath, "clicks.txt");
        
        if (File.Exists(_filePath))
        {
            _currentClicks = LoadClicks();
        }
        else
        {
            SaveClicks();
        }

        OnClick?.Invoke(_currentClicks);
        
        Debug.Log($"Loaded clicks: {_currentClicks}");
        
        _originalScale = _spriteRenderer.transform.localScale;
        _originalAlpha = _spriteRenderer.color.a;
    }

    private void OnApplicationQuit()
    {
        SaveClicks();
        Debug.Log($"Saved clicks: {_currentClicks}");
    }
    private void OnDestroy()
    {
        SaveClicks();
        Debug.Log($"Saved clicks: {_currentClicks}");
    }
    #endregion

    #region Save/Load 
    private void SaveClicks()
    {
        try
        {
            File.WriteAllText(_filePath, _currentClicks.ToString());
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error saving clicks: {ex.Message}");
        }
    }

    private int LoadClicks()
    {
        try
        {
            string data = File.ReadAllText(_filePath);
            return int.Parse(data);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading clicks: {ex.Message}");
            return 0;
        }
    }
    
    #endregion
    
    #region DoTweens
    
    private async UniTask PlayClickAnimation()
    {
        if (_spriteRenderer == null)
        {
            Debug.LogError("Animation targets are not assigned.");
            return;
        }
        
        Vector3 originalScale = _originalScale;
        float originalAlpha = _originalAlpha;
        
        await UniTask.WhenAll(
            transform.DOScale(originalScale * scaleMultiplier, animationDuration / 2).SetEase(Ease.OutQuad).AsyncWaitForCompletion(),
            _spriteRenderer.DOFade(0, animationDuration / 2).SetEase(Ease.OutQuad).AsyncWaitForCompletion()
        );

        // Анимация: возврат к исходным значениям
        await UniTask.WhenAll(
            transform.DOScale(originalScale, animationDuration / 2).SetEase(Ease.InQuad).AsyncWaitForCompletion(),
            _spriteRenderer.DOFade(originalAlpha, animationDuration / 2).SetEase(Ease.InQuad).AsyncWaitForCompletion()
        );
        
        
    }
    
    private async UniTask PlayParticleEffect(Vector2 position)
    {
        if (particlePrefab == null)
        {
            Debug.LogError("Particle prefab is not assigned.");
            return;
        }

        List<UniTask> particleTasks = new List<UniTask>();
        List<GameObject> particles = new List<GameObject>();
        
        for (int i = 0; i < particleCount; i++)
        {
            GameObject particle = Instantiate(particlePrefab, position, Quaternion.identity);
            Transform particleTransform = particle.GetComponent<Transform>();

            particles.Add(particle);
            
            Vector2 randomDirection = Random.insideUnitCircle.normalized * spreadRadius;
            
            particleTasks.Add(UniTask.WhenAll(
                particleTransform.DOMove((Vector2)particleTransform.position + randomDirection, _particleAnimationDuration)
                    .OnKill(() => Debug.Log("Tween killed because the object was destroyed."))
                    .SetEase(Ease.OutQuad).AsyncWaitForCompletion(),
                particle.GetComponent<SpriteRenderer>().DOFade(0,_particleAnimationDuration)
                    .OnKill(() => Debug.Log("Tween killed because the object was destroyed."))
                    .SetEase(Ease.InQuad).AsyncWaitForCompletion()
            ));
            
        }

        await UniTask.WhenAll(particleTasks);
        
        foreach (var go in particles)
        {
            Destroy(go);
        }
    }
    
    #endregion
    
    #region Gameplay Methods
    internal void Click(Vector2 position)
    {
        _currentClicks++;
        
        OnClick?.Invoke(_currentClicks);
        
        UniTask.WhenAll(PlayClickAnimation(),PlayParticleEffect(position));
        
    }
    #endregion
    
}
