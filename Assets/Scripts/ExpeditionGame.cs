using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class ExpeditionGame : MonoBehaviour
{
    enum Terrain { Sea, Plains, Forest, Desert, Mountain }
    sealed class Site { public Vector2Int p; public string name; public int gold; public bool found; public Color color; }

    const int W = 34, H = 22;
    readonly Dictionary<Vector2Int, GameObject> fog = new();
    readonly List<Site> sites = new();
    Terrain[,] map;
    Vector2Int pos;
    int provisions, morale, gold, discoveries, days;
    bool title = true, ended, won, paused;
    float moveCooldown;
    Text hud, message, objective;
    GameObject explorer, titlePanel, endPanel, pausePanel;
    Camera cam;
    Sprite square, circle;
    System.Random rng;

    readonly Color parchment = new(0.89f, 0.82f, 0.64f), ink = new(0.12f, 0.15f, 0.12f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot() { if (FindFirstObjectByType<ExpeditionGame>() == null) new GameObject("Atlas of the Seventh Sun").AddComponent<ExpeditionGame>(); }

    void Awake()
    {
        Application.targetFrameRate = 60;
        square = MakeSprite(false); circle = MakeSprite(true);
        cam = Camera.main ?? new GameObject("Main Camera").AddComponent<Camera>();
        cam.tag = "MainCamera"; cam.orthographic = true; cam.orthographicSize = 8.2f; cam.backgroundColor = new Color(0.055f, .07f, .065f); cam.transform.position = new Vector3(W/2f, H/2f, -10);
        BuildUI(); ShowTitle();
    }

    Sprite MakeSprite(bool round)
    {
        var t = new Texture2D(32, 32, TextureFormat.RGBA32, false); t.filterMode = FilterMode.Point;
        for (int y=0;y<32;y++) for(int x=0;x<32;x++) { float d=Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f)); t.SetPixel(x,y,!round||d<15?Color.white:Color.clear); }
        t.Apply(); return Sprite.Create(t,new Rect(0,0,32,32),new Vector2(.5f,.5f),32);
    }

    void BuildUI()
    {
        var canvas = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler=canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1440,900);
        hud = Label(canvas.transform, "HUD", "", 21, TextAnchor.UpperLeft, new Vector2(22,-18), new Vector2(620,120));
        objective = Label(canvas.transform, "Objective", "", 18, TextAnchor.UpperRight, new Vector2(-22,-18), new Vector2(600,150)); objective.rectTransform.anchorMin=objective.rectTransform.anchorMax=new Vector2(1,1); objective.rectTransform.pivot=new Vector2(1,1);
        message = Label(canvas.transform, "Message", "", 22, TextAnchor.LowerCenter, new Vector2(0,28), new Vector2(900,70)); message.rectTransform.anchorMin=message.rectTransform.anchorMax=new Vector2(.5f,0); message.rectTransform.pivot=new Vector2(.5f,0);
        titlePanel = Panel(canvas.transform,"Title",new Color(.055f,.07f,.065f,.97f));
        Label(titlePanel.transform,"TitleText","ATLAS OF THE SEVENTH SUN",52,TextAnchor.MiddleCenter,new Vector2(0,115),new Vector2(1100,90));
        Label(titlePanel.transform,"Sub","Chart the unknown. Trade with honor. Return with a legend.\n\nWASD / ARROWS — travel     E — interact     ESC — pause",24,TextAnchor.MiddleCenter,new Vector2(0,-5),new Vector2(1050,180));
        MakeButton(titlePanel.transform,"Begin Expedition",new Vector2(0,-150),StartGame);
        pausePanel=Panel(canvas.transform,"Pause",new Color(.03f,.04f,.035f,.9f)); Label(pausePanel.transform,"PauseText","EXPEDITION PAUSED\n\nPress ESC to continue",38,TextAnchor.MiddleCenter,Vector2.zero,new Vector2(800,220)); pausePanel.SetActive(false);
        endPanel=Panel(canvas.transform,"End",new Color(.03f,.04f,.035f,.96f)); endPanel.SetActive(false);
    }

    Text Label(Transform parent,string name,string value,int size,TextAnchor anchor,Vector2 pos,Vector2 dims)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false); var r=go.GetComponent<RectTransform>(); r.anchorMin=r.anchorMax=new Vector2(.5f,.5f); r.sizeDelta=dims; r.anchoredPosition=pos;
        var t=go.GetComponent<Text>(); t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.text=value; t.fontSize=size; t.alignment=anchor; t.color=parchment; t.fontStyle=FontStyle.Bold; t.horizontalOverflow=HorizontalWrapMode.Wrap; return t;
    }
    GameObject Panel(Transform parent,string name,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;go.GetComponent<Image>().color=color;return go;}
    void MakeButton(Transform p,string text,Vector2 pos,UnityEngine.Events.UnityAction action){var go=new GameObject(text,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(p,false);var r=go.GetComponent<RectTransform>();r.sizeDelta=new Vector2(330,68);r.anchoredPosition=pos;go.GetComponent<Image>().color=new Color(.67f,.42f,.16f);go.GetComponent<Button>().onClick.AddListener(action);var l=Label(go.transform,"Text",text,24,TextAnchor.MiddleCenter,Vector2.zero,r.sizeDelta);l.color=Color.white;}

    void ShowTitle(){ hud.gameObject.SetActive(false); objective.gameObject.SetActive(false); message.gameObject.SetActive(false); titlePanel.SetActive(true); }
    void StartGame(){ title=false;ended=false;won=false;titlePanel.SetActive(false);endPanel.SetActive(false);hud.gameObject.SetActive(true);objective.gameObject.SetActive(true);message.gameObject.SetActive(true);Generate(); }

    void Generate()
    {
        foreach(var s in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)) Destroy(s.gameObject);
        fog.Clear();sites.Clear(); rng=new System.Random(Environment.TickCount); map=new Terrain[W,H];
        for(int x=0;x<W;x++)for(int y=0;y<H;y++) { float n=Mathf.PerlinNoise(x*.14f+3.7f,y*.14f+8.1f); Terrain t=x<4?Terrain.Sea:n<.32f?Terrain.Desert:n<.47f?Terrain.Plains:n<.66f?Terrain.Forest:Terrain.Mountain; map[x,y]=t; Tile(x,y,t); }
        pos=new Vector2Int(3,H/2); provisions=90;morale=70;gold=0;discoveries=0;days=1;
        AddSite(new Vector2Int(8,6),"River Confederacy",20,new Color(.18f,.52f,.45f)); AddSite(new Vector2Int(16,16),"Sunstone Mission",28,new Color(.79f,.7f,.3f)); AddSite(new Vector2Int(25,7),"Red Mesa Citadel",35,new Color(.65f,.25f,.17f)); AddSite(new Vector2Int(30,17),"The Seventh City",90,new Color(1f,.72f,.16f));
        var p=new GameObject("Explorer",typeof(SpriteRenderer));explorer=p;p.GetComponent<SpriteRenderer>().sprite=circle;p.GetComponent<SpriteRenderer>().color=new Color(1f,.78f,.25f);p.transform.localScale=Vector3.one*.65f;p.GetComponent<SpriteRenderer>().sortingOrder=5;
        Reveal(); SetMessage("Your ship anchors at Dawnwatch. Inland, the map is blank."); UpdateView();
    }
    void Tile(int x,int y,Terrain t){var go=new GameObject($"{t}_{x}_{y}",typeof(SpriteRenderer));go.transform.position=new Vector3(x,y,0);var sr=go.GetComponent<SpriteRenderer>();sr.sprite=square;sr.color=t switch{Terrain.Sea=>new Color(.12f,.3f,.34f),Terrain.Plains=>new Color(.52f,.55f,.31f),Terrain.Forest=>new Color(.17f,.32f,.22f),Terrain.Desert=>new Color(.69f,.55f,.32f),_=>new Color(.37f,.36f,.32f)};go.transform.localScale=Vector3.one*.96f;var f=new GameObject("Uncharted",typeof(SpriteRenderer));f.transform.position=new Vector3(x,y,-.2f);var fs=f.GetComponent<SpriteRenderer>();fs.sprite=square;fs.color=new Color(.035f,.045f,.04f,.97f);fs.sortingOrder=3;fog[new Vector2Int(x,y)]=f;}
    void AddSite(Vector2Int p,string name,int value,Color c){sites.Add(new Site{p=p,name=name,gold=value,color=c});var go=new GameObject(name,typeof(SpriteRenderer));go.transform.position=new Vector3(p.x,p.y,-.1f);go.transform.localScale=Vector3.one*.55f;var sr=go.GetComponent<SpriteRenderer>();sr.sprite=circle;sr.color=c;sr.sortingOrder=2;}

    void Update()
    {
        if(title||ended)return; var k=Keyboard.current;if(k==null)return;
        if(k.escapeKey.wasPressedThisFrame){paused=!paused;pausePanel.SetActive(paused);} if(paused)return;
        moveCooldown-=Time.deltaTime; Vector2Int d=Vector2Int.zero;if(k.wKey.isPressed||k.upArrowKey.isPressed)d=Vector2Int.up;else if(k.sKey.isPressed||k.downArrowKey.isPressed)d=Vector2Int.down;else if(k.aKey.isPressed||k.leftArrowKey.isPressed)d=Vector2Int.left;else if(k.dKey.isPressed||k.rightArrowKey.isPressed)d=Vector2Int.right;
        if(d!=Vector2Int.zero&&moveCooldown<=0){Move(d);moveCooldown=.13f;} if(k.eKey.wasPressedThisFrame)Interact();
    }
    void Move(Vector2Int d){var n=pos+d;if(n.x<1||n.x>=W-1||n.y<1||n.y>=H-1)return;int cost=map[n.x,n.y] switch{Terrain.Mountain=>3,Terrain.Forest=>2,_=>1};pos=n;days++;provisions-=cost;if(rng.NextDouble()<.08){morale-=4;SetMessage("Hard country tests the company. Morale falls.");}Reveal();UpdateView();if(provisions<=0||morale<=0)Finish(false,"The expedition can go no farther.");}
    void Reveal(){for(int x=pos.x-2;x<=pos.x+2;x++)for(int y=pos.y-2;y<=pos.y+2;y++)if(fog.TryGetValue(new Vector2Int(x,y),out var f)){Destroy(f);fog.Remove(new Vector2Int(x,y));}}
    void Interact(){var site=sites.Find(s=>Vector2Int.Distance(s.p,pos)<1.5f&&!s.found);if(site!=null){site.found=true;discoveries++;gold+=site.gold;morale=Mathf.Min(100,morale+10);SetMessage($"{site.name} entered the atlas. Tribute and trade yield {site.gold} gold.");}else if(pos.x<=4){provisions=Mathf.Min(90,provisions+45);morale=Mathf.Min(100,morale+15);SetMessage("Dawnwatch resupplies the company.");if(gold>=120&&discoveries>=4)Finish(true,"You return with the Seventh City mapped and a legend secured.");}else SetMessage("There is nothing here to negotiate with. Keep charting.");UpdateView();}
    void UpdateView(){if(explorer)explorer.transform.position=new Vector3(pos.x,pos.y,-.3f);cam.transform.position=new Vector3(Mathf.Clamp(pos.x,8,W-8),Mathf.Clamp(pos.y,6,H-6),-10);hud.text=$"DAY {days}    PROVISIONS {provisions}    MORALE {morale}    GOLD {gold}";objective.text=$"THE MANDATE\nDiscover all four sites: {discoveries}/4\nSecure 120 gold: {gold}/120\nReturn to Dawnwatch in the west\n\nPress E at sites or port";}
    void SetMessage(string s){message.text=s;CancelInvoke(nameof(ClearMessage));Invoke(nameof(ClearMessage),5);}void ClearMessage(){if(message)message.text="";}
    void Finish(bool victory,string reason){ended=true;won=victory;endPanel.SetActive(true);foreach(Transform c in endPanel.transform)Destroy(c.gameObject);Label(endPanel.transform,"Result",(victory?"THE ATLAS IS COMPLETE":"THE EXPEDITION IS LOST")+"\n\n"+reason+$"\n\nDays {days}   Gold {gold}   Discoveries {discoveries}/4",40,TextAnchor.MiddleCenter,new Vector2(0,70),new Vector2(1000,320));MakeButton(endPanel.transform,"Sail Again",new Vector2(0,-150),StartGame);}
}

