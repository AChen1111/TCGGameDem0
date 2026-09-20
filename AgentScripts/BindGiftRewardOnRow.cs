using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BindGiftRewardOnRow
{
    const string RowPath = "Assets/UI/Prefab/Hall/PreGameUI/Meau/GiftWindow/GiftApplyRowPrefab.prefab";

    public static string Run()
    {
        GameObject row = PrefabUtility.LoadPrefabContents(RowPath);
        try
        {
            var log = new StringBuilder();
            for (int i = 0; i < row.transform.childCount; i++)
            {
                log.Append(row.transform.GetChild(i).name).Append(',');
            }

            Transform reward = row.transform.Find("Reward");
            if (reward == null)
            {
                throw new System.InvalidOperationException("找不到 Reward: " + log);
            }

            var item = reward.GetComponent<GiftRewardItem>();
            if (item == null)
            {
                item = reward.gameObject.AddComponent<GiftRewardItem>();
            }

            Image gold = FindImage(reward, "Img_gold", "Img_icon");
            RawImage card = FindRaw(reward, "Img_card");
            TMP_Text count = FindTmp(reward, "Txt_count");
            SerializedObject itemSo = new SerializedObject(item);
            itemSo.FindProperty("m_ImgGold").objectReferenceValue = gold;
            itemSo.FindProperty("m_ImgCard").objectReferenceValue = card;
            itemSo.FindProperty("m_TxtCount").objectReferenceValue = count;
            itemSo.ApplyModifiedPropertiesWithoutUndo();

            var rowItem = row.GetComponent<GiftRewardRowItem>();
            SerializedObject rowSo = new SerializedObject(rowItem);
            rowSo.FindProperty("m_Reward").objectReferenceValue = item;
            rowSo.ApplyModifiedPropertiesWithoutUndo();

            bool success;
            PrefabUtility.SaveAsPrefabAsset(row, RowPath, out success);
            if (!success)
            {
                throw new System.InvalidOperationException("SaveAsPrefabAsset 失败: " + log);
            }

            EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<GameObject>(RowPath));
            AssetDatabase.SaveAssets();
            return "bound;" + log + "gold=" + (gold != null) + ";card=" + (card != null) + ";count=" + (count != null);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(row);
        }
    }

    static Image FindImage(Transform parent, params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            Transform child = parent.Find(names[i]);
            if (child != null)
            {
                return child.GetComponent<Image>();
            }
        }

        return parent.GetComponentInChildren<Image>(true);
    }

    static RawImage FindRaw(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        return child != null ? child.GetComponent<RawImage>() : parent.GetComponentInChildren<RawImage>(true);
    }

    static TMP_Text FindTmp(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        return child != null ? child.GetComponent<TMP_Text>() : parent.GetComponentInChildren<TMP_Text>(true);
    }
}
