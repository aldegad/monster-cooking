using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThumbnailGenerator : MonoBehaviour
{
    [SerializeField] private string thumbnailPath = "/ThumbnailGenerator/Thumbnails";
    [SerializeField] private Camera thumbnailCamera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private TMP_InputField thumbnailNameInput;
    [SerializeField] private Button thumbnailButton;

    private void Start()
    {
        thumbnailButton.onClick.AddListener(() =>
        {
            GenerateThumbnail();
        });
    }

    public void GenerateThumbnail()
    {
        // RenderTexture에 카메라의 뷰를 렌더링
        thumbnailCamera.targetTexture = renderTexture;
        thumbnailCamera.Render();

        // RenderTexture의 내용을 Texture2D로 복사
        RenderTexture.active = renderTexture;
        Texture2D thumbnailTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        thumbnailTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        thumbnailTexture.Apply();

        // 이미지 파일로 저장
        byte[] bytes = thumbnailTexture.EncodeToPNG();

        string path = Application.dataPath + thumbnailPath + "/" + thumbnailNameInput.text + ".png";
        Debug.Log(path);
        System.IO.File.WriteAllBytes(path, bytes);

        // 사용 후 정리
        RenderTexture.active = null;
    }
}
