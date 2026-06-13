using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// Resources 资源加载管理器
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c>LoadEditorAsset&lt;T&gt;(assetName)</c>：加载单个资源 </description>
/// </item>
/// <item>
/// <description><c>LoadAtlasSprite(atlasName, spriteName)</c>：加载图集中的某一张Sprite </description>
/// </item>
/// <item>
/// <description><c>LoadSpriteAtlas(atlasName)</c>：加载整个图集，返回 Key是SpriteName ，Value是 Sprite 的字典 </description>
/// </item>
/// <item>
/// 强行规定资源必须放在"Assets/Editor/ArtRes/"
/// </item>
/// /// <item>
/// 强行规定图集后缀必须是".png"
/// </item>
/// </list>
/// </remarks>
public class XEditorAssetManager : XSingletonCSharp<XEditorAssetManager>
{
    private XEditorAssetManager()
    {
    }

    //强行规定资源必须放在"Assets/Editor/ArtRes/"
    private string _rootPath = "Assets/Editor/ArtRes/";

    //1. 加载单个资源
    public T LoadEditorAsset<T>(string assetName) where T : Object
    {
        string assetSuffix = "";

        if (typeof(T) == typeof(GameObject))
        {
            assetSuffix = ".prefab";
        }
        else if (typeof(T) == typeof(Sprite))
        {
            assetSuffix = ".png";
        }
        else if (typeof(T) == typeof(Material))
        {
            assetSuffix = ".mat";
        }
        else if (typeof(T) == typeof(AudioClip))
        {
            assetSuffix = ".mp3";
        }

        var asset = AssetDatabase.LoadAssetAtPath<T>(_rootPath + assetName + assetSuffix);
        if (asset == null)
        {
            Debug.LogError("加载失败 " + assetName);
            return null;
        }

        return asset;
    }

    //2.加载图集中的某个Sprite
    public Sprite LoadAtlasSprite(string atlasName, string spriteName)
    {
        //强行规定图集后缀必须是".png"
        var atlas = AssetDatabase.LoadAllAssetsAtPath(_rootPath + atlasName + ".png");
        if (atlas == null)
        {
            Debug.LogError($"未找到{atlasName}，加载失败");
            return null;
        }

        foreach (var sprite in atlas)
        {
            if (sprite.name == spriteName)
            {
                if (sprite is Sprite s && s.name == spriteName)
                {
                    return s;
                }
            }
        }

        Debug.LogError($"未找到{spriteName}，加载失败");
        return null;
    }

    //3.加载某个图集，并返回字典
    public Dictionary<string, Sprite> LoadSpriteAtlas(string atlasName)
    {
        Dictionary<string, Sprite> atlas = new Dictionary<string, Sprite>();
        //强行规定图集后缀必须是".png"
        var atlasObjects = AssetDatabase.LoadAllAssetsAtPath(_rootPath + atlasName + ".png");
        if (atlasObjects == null)
        {
            Debug.LogError($"未找到{atlasName}，加载失败");
            return null;
        }

        for (int i = 0; i < atlasObjects.Length; i++)
        {
            if (atlasObjects[i] is Sprite sprite)
            {
                atlas.Add(atlasObjects[i].name, sprite);
            }
        }

        return atlas;
    }
}