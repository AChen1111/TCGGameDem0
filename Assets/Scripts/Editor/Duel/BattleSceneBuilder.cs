using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AChen.Configuration;
using AChen.Decks;
using AChen.Duel.Presentation;
using Newtonsoft.Json;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>只组装指定原图、现有卡图和界面引用。</summary>
public static partial class BattleSceneBuilder
{
    const string Prefabs = "Assets/UI/Prefab/Battle/";
    const string Art = "Assets/Art/Battle/";
    const string Sprites = "Assets/UI/Sprite/Battle/";
    const string Scripts = "Assets/Scripts/Duel/Presentation";
    static TMP_FontAsset s_font;
    static Material s_card;
    static Material s_glow;
    static Material s_back, s_worldText;
    sealed class ImportEntry { public string name; public Vector2 pivot; public Vector4 border; public float pixelsPerUnit; }
    static T Root<T>(GameObject go) where T:Component
    {
        var array=new SerializedObject(go).FindProperty("m_Component");
        return Enumerable.Range(0,array.arraySize).Select(i=>array.GetArrayElementAtIndex(i).FindPropertyRelative("component").objectReferenceValue).OfType<T>().Single();
    }
    static void Ref(UnityEngine.Object target,string field,UnityEngine.Object value)
    {var so=new SerializedObject(target);so.FindProperty(field).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    static void Refs(UnityEngine.Object target,string field,IEnumerable<UnityEngine.Object> values)
    {var so=new SerializedObject(target);var p=so.FindProperty(field);var v=values.ToArray();p.arraySize=v.Length;
     for(int i=0;i<v.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=v[i];so.ApplyModifiedPropertiesWithoutUndo();}
    static void Int(UnityEngine.Object target,string field,int value)
    {var so=new SerializedObject(target);so.FindProperty(field).intValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    static GameObject Save(GameObject go,string name)
    {var prefab=PrefabUtility.SaveAsPrefabAsset(go,Prefabs+name+".prefab");UnityEngine.Object.DestroyImmediate(go);return prefab;}
    static T Store<T>(T asset,string path) where T:UnityEngine.Object
    {
        asset.name=Path.GetFileNameWithoutExtension(path);
        if(File.Exists(path)){var existing=AssetDatabase.LoadAssetAtPath<T>(path);EditorUtility.CopySerialized(asset,existing);
            UnityEngine.Object.DestroyImmediate(asset);EditorUtility.SetDirty(existing);return existing;}
        AssetDatabase.CreateAsset(asset,path);return asset;
    }
    static void DisplayAssets()
    {
        s_font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/FZZYJW SDF.asset");
        s_card=Store(new Material(Shader.Find("TCG/Battle/CardSurface")){name="BattleCardSurface"},Art+"BattleCardSurface.mat");
        s_glow=Store(new Material(Shader.Find("TCG/Battle/CardGlow")){name="BattleCardGlow"},Art+"BattleCardGlow.mat");
        var back=new Material(Shader.Find("TCG/Battle/CardSurface"));back.SetTexture("_BaseMap",Sprite("DefaultProtector").texture);
        s_back=Store(back,Art+"BattleCardBack.mat");
        var worldText=new Material(s_font.material);worldText.SetFloat("_ZTestMode",8);worldText.renderQueue=3100;
        s_worldText=Store(worldText,Art+"BattleWorldText.mat");
    }
    static Transform Obj(string name,Transform parent,Vector3 position)
    {var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;return go.transform;}
    static SpriteRenderer Picture(string name,Transform parent,Vector3 position,float width,float height,Sprite sprite,int order=0)
    {
        var t=Obj(name,parent,position);t.localRotation=Quaternion.Euler(90,0,0);
        t.localScale=new Vector3(width/sprite.bounds.size.x,height/sprite.bounds.size.y,1);
        var renderer=t.gameObject.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=s_card;renderer.sortingOrder=order;
        var properties=new MaterialPropertyBlock();properties.SetTexture("_BaseMap",sprite.texture);renderer.SetPropertyBlock(properties);return renderer;
    }
    static TextMeshPro WorldText(string name,Transform parent,Vector3 position,string value,float size)
    {
        var t=Obj(name,parent,position);t.localRotation=Quaternion.Euler(65,0,0);
        var label=t.gameObject.AddComponent<TextMeshPro>();label.font=s_font;label.text=value;label.fontSize=size*12;
        label.alignment=TextAlignmentOptions.Center;label.color=new Color(.95f,.94f,.8f);label.rectTransform.sizeDelta=new Vector2(3.4f,1.2f);return label;
    }
    static BattleHighlightView Surface(string name,Transform parent,Vector3 position,float width,float height,Sprite sprite,bool hide=false,int order=2)
    {
        var renderer=Picture(name,parent,position,width,height,sprite,order);var highlight=renderer.gameObject.AddComponent<BattleHighlightView>();
        Ref(highlight,"m_surface",renderer);Ref(highlight,"m_texture",sprite.texture);
        var so=new SerializedObject(highlight);so.FindProperty("m_hideInactive").boolValue=hide;so.ApplyModifiedPropertiesWithoutUndo();return highlight;
    }
    static void ImportSprites()
    {
        foreach(var entry in JsonConvert.DeserializeObject<ImportEntry[]>(File.ReadAllText(Sprites+"import-metadata.json")))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Sprites+entry.name+".png");
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePivot=entry.pivot;importer.spriteBorder=entry.border;importer.spritePixelsPerUnit=entry.pixelsPerUnit;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=2048;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
    }
    static Sprite Sprite(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+name+".png");
    static GameObject BuildCard()
    {
        var original=PrefabUtility.LoadPrefabContents(MDPro3BattleImport.Folder+"/GameObject/CardModelWrapper.prefab");
        var root=Obj("BattleCardView",null,Vector3.zero);var view=root.gameObject.AddComponent<BattleCardView>();
        var plane=UnityEngine.Object.Instantiate(original.transform.Find("CardPlane").gameObject,root).transform;
        plane.name="CardPlane";plane.localPosition=Vector3.zero;
        var pivot=plane.Find("Pivot");var offset=pivot.Find("Offset");var turn=offset.Find("Turn");var model=turn.Find("CardModel").gameObject;
        var face=Root<MeshRenderer>(model.transform.Find("CardModel_front").gameObject);face.sharedMaterial=s_card;
        var surface=face.gameObject.AddComponent<BattleHighlightView>();Ref(surface,"m_surface",face);Ref(surface,"m_texture",Sprite("DefaultProtector").texture);
        var back=Root<MeshRenderer>(model.transform.Find("CardModel_back").gameObject);back.sharedMaterial=s_card;
        var backSurface=back.gameObject.AddComponent<BattleHighlightView>();Ref(backSurface,"m_surface",back);Ref(backSurface,"m_texture",Sprite("DefaultProtector").texture);
        var glow=UnityEngine.Object.Instantiate(face.gameObject,model.transform);glow.name="CardOuterGlow";
        UnityEngine.Object.DestroyImmediate(Root<BattleHighlightView>(glow));var glowRenderer=Root<MeshRenderer>(glow);glowRenderer.sharedMaterial=s_glow;
        var outline=glow.AddComponent<BattleHighlightView>();Ref(outline,"m_surface",glowRenderer);Ref(outline,"m_texture",Sprite("DefaultProtector").texture);
        var box=model.AddComponent<BoxCollider>();box.size=new Vector3(5.9f,.35f,8.6f);
        foreach(var particle in SceneComponents<ParticleSystem>(plane))particle.gameObject.SetActive(false);
        Ref(view,"m_surface",surface);Ref(view,"m_backSurface",backSurface);Ref(view,"m_outline",outline);
        Ref(view,"m_hitbox",box);Ref(view,"m_back",Sprite("DefaultProtector").texture);
        Ref(view,"m_cardPlane",plane);Ref(view,"m_pivot",pivot);Ref(view,"m_offset",offset);Ref(view,"m_turn",turn);
        var statistics=WorldText("Statistics",root,new Vector3(0,.15f,-5.3f),"",.8f);statistics.rectTransform.sizeDelta=new Vector2(8,3);Ref(view,"m_stats",statistics);
        PrefabUtility.UnloadPrefabContents(original);return Save(root.gameObject,"BattleCardView");
    }
    static DuelDemoPreset BuildPreset()
    {
        var cards=Table.CardRow.LoadBytes(File.ReadAllBytes("Assets/GameConfiguration/Cards.bytes"));
        var rules=DeckRulesConfiguration.Create(cards.Select(c=>c.CardId),BinaryTable.Decode(File.ReadAllBytes("Assets/GameConfiguration/card-deck-sections.bytes")),
            BinaryTable.Decode(File.ReadAllBytes("Assets/GameConfiguration/card-banlist.bytes")));
        string Pool(string id)=>new[]{("Card01",CardPoolAddress.Card01),("Card02",CardPoolAddress.Card02),("Card03",CardPoolAddress.Card03),("Card04",CardPoolAddress.Card04)}
            .First(pair=>File.Exists("Assets/UI/Card/"+pair.Item2+"/"+id+".jpg")).Item1;
        var candidates=cards.Where(c=>rules.GetMaxCopies(c.CardId)>0 && new[]{CardPoolAddress.Card01,CardPoolAddress.Card02,CardPoolAddress.Card03,CardPoolAddress.Card04}
            .Any(folder=>File.Exists("Assets/UI/Card/"+folder+"/"+c.CardId+".jpg"))).ToArray();
        DuelDemoCardEntry[] Recipe(bool extra,int total)
        {
            var available=candidates.Where(c=>rules.TryGetSection(c.CardId,out var section)&&section==(extra?DeckSection.Extra:DeckSection.Main))
                .OrderBy(c=>c.Kind).ThenBy(c=>c.Frame).ThenBy(c=>c.Level).ThenBy(c=>c.CardId,StringComparer.Ordinal).ToArray();
            var order=new List<Table.CardRow>();
            if(!extra)
            {order.Add(available.First(c=>c.Kind==1&&c.Frame==1));order.Add(available.First(c=>c.Kind==1&&c.Frame==2));
             order.Add(available.First(c=>c.Kind==2));order.Add(available.First(c=>c.Kind==3));}
            else foreach(int frame in new[]{4,5,6,7}) order.Add(available.First(c=>c.Frame==frame));
            order.AddRange(available.Where(c=>!order.Contains(c)));var entries=new List<DuelDemoCardEntry>();int count=0;
            foreach(var c in order)
            {int copies=Math.Min(rules.GetMaxCopies(c.CardId),Math.Min(extra||entries.Count<4?1:3,total-count));if(copies==0)break;
             entries.Add(new DuelDemoCardEntry{CardId=c.CardId,SourcePool=Pool(c.CardId),Count=copies});count+=copies;}
            return entries.ToArray();
        }
        var main=Recipe(false,40);var extraEntries=Recipe(true,15);
        var deck=new DeckData(Guid.Empty,"森林遗迹演示",main.Select(c=>new DeckCardEntry(c.CardId,0,c.Count)),extraEntries.Select(c=>new DeckCardEntry(c.CardId,0,c.Count)));
        var result=DeckValidator.Validate(deck,rules,deck.MainDeck.Concat(deck.ExtraDeck).ToArray(),DeckValidationMode.Playable);
        if(!result.IsValid)throw new InvalidOperationException("演示构筑不符合项目配置");
        var preset=ScriptableObject.CreateInstance<DuelDemoPreset>();var so=new SerializedObject(preset);
        void Entries(string field,DuelDemoCardEntry[] entries)
        {var array=so.FindProperty(field);array.arraySize=entries.Length;for(int i=0;i<entries.Length;i++)
         {var e=array.GetArrayElementAtIndex(i);e.FindPropertyRelative("CardId").stringValue=entries[i].CardId;
          e.FindPropertyRelative("SourcePool").stringValue=entries[i].SourcePool;e.FindPropertyRelative("Count").intValue=entries[i].Count;}}
        Entries("m_main",main);Entries("m_extra",extraEntries);
        var players=so.FindProperty("m_players");players.arraySize=2;
        for(int i=0;i<2;i++){var p=players.GetArrayElementAtIndex(i);p.FindPropertyRelative("Name").stringValue=i==0?"演示玩家":"演示对手";
            p.FindPropertyRelative("AvatarId").intValue=1010001+i;p.FindPropertyRelative("LP").intValue=8000;}
        so.ApplyModifiedPropertiesWithoutUndo();
        return Store(preset,Art+"BattleDemoPreset.asset");
    }
}
