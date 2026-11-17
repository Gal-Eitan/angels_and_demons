# הרצת המשחק

תחילה מוצגת מפת רומא והשחקן צריך לבחור במיקום אליו ללכת לחפש את הקרדינל שנחטף, על פי הרמז בפינה.
בחירה שגויה מפחיתה מהזמן שיהיה להציל את הקרדינל(עקב הזמן שבוזבז על הליכה למקום הלא נכון).
בחירה במיקום הנכון פותחת את הרמה עצמה ומתחילה את הטיימר.
ברמה עצמה השחקן צריך להגיע לקרדינל ולנסות להצילו בזמן(בשלב זה הרוצח משתמש ב"יסוד האדמה" כדי להרוג אותו).

# יחסי המחלקות
מערכת המשחק בנויה בארכיטקטורה מבוזרת (Decoupled Architecture) המשלבת **ScriptableObjects** לשיתוף נתונים ולהעברת אירועים (Event Channels), יחד עם **UnityEvents** לאינטראקציות מקומיות בסצנה. ארכיטקטורה זו מאפשרת לרכיבי הלוגיקה, הפיזיקה וממשק המשתמש (UI) לתקשר ביניהם ללא תלות ישירה (Tight Coupling) זה בזה.

### 1. תרשים מחלקות וקשרים (UML / Architecture Diagram)

```
classDiagram
    direction TB

    %% ScriptableObjects - Data & Config
    class LevelConfigSO {
        <<ScriptableObject>>
        +float startingTime
        +float wrongMapPenalty
        +float rockHitPenalty
    }

    class LevelData {
        <<ScriptableObject>>
        +bool isGateOpen
        +bool isCardinalSafe
        +bool isPlayerDead
        +bool isTimeUp
        +ResetData()
    }

    %% ScriptableObjects - Event Channels
    class VoidEventChannelSO {
        <<ScriptableObject>>
        +UnityAction OnEventRaised
        +RaiseEvent()
    }

    class FloatEventChannelSO {
        <<ScriptableObject>>
        +UnityAction~float~ OnEventRaised
        +RaiseEvent(float)
    }

    %% Core & UI Controllers
    class MapRiddleController {
        -LevelConfigSO levelConfig
        -FloatEventChannelSO timePenaltyChannel
        -VoidEventChannelSO levelStartedChannel
        +SelectLocation(int)
    }

    class LevelTimer {
        -LevelConfigSO levelConfig
        -LevelData levelData
        -VoidEventChannelSO levelStartedChannel
        -FloatEventChannelSO timePenaltyChannel
        -FloatEventChannelSO timeUpdatedChannel
        -VoidEventChannelSO timeExpiredChannel
        -StartTimer()
        -ApplyPenalty(float)
        -timeUp()
    }

    class TimerUIView {
        -FloatEventChannelSO timeUpdatedChannel
        -UpdateDisplay(float)
    }

    class showResults {
        -LevelData levelData
        +playAgain()
    }

    %% Player & Physics
    class PlayerController {
        +UnityEvent onInteract
        -VoidEventChannelSO levelStartedChannel
        -EnableControl()
        +StartInteract()
    }

    class StairSwitch {
        -Collider2D upperStairsCollider
        -OnTriggerEnter2D(Collider2D)
    }

    %% World Interactors & Hazards
    class AngelInteractor {
        -GameObject player
        -GameObject reflection
        -GameObject boulder
        +correctMirror()
    }

    class gateOpener {
        -GameObject player
        -LevelData levelData
        +openGate()
    }

    class CardinalLifter {
        -GameObject player
        -LevelData levelData
        +LiftCardinal()
    }

    class FallingDetector {
        -LevelData levelData
        -OnTriggerEnter2D(Collider2D)
    }

    class PlayerDetector {
        -UnityEvent OnPlayerDetect
        -Update()
    }

    class SpawnedStone {
        -VoidEventChannelSO stoneDestroyedChannel
        -FloatEventChannelSO stoneHittedPlayer
        -OnCollisionEnter2D(Collision2D)
        -OnDestroy()
    }

    %% Event Broadcasting (Senders)
    MapRiddleController ..> VoidEventChannelSO : Raises (levelStarted)
    MapRiddleController ..> FloatEventChannelSO : Raises (wrongMapPenalty)
    LevelTimer ..> FloatEventChannelSO : Raises (timeUpdated)
    LevelTimer ..> VoidEventChannelSO : Raises (timeExpired)
    SpawnedStone ..> FloatEventChannelSO : Raises (stoneHittedPlayer)
    SpawnedStone ..> VoidEventChannelSO : Raises (stoneDestroyed)

    %% Event Listening (Receivers)
    VoidEventChannelSO --> PlayerController : Notifies (EnableControl)
    VoidEventChannelSO --> LevelTimer : Notifies (StartTimer)
    FloatEventChannelSO --> LevelTimer : Notifies (ApplyPenalty)
    FloatEventChannelSO --> TimerUIView : Notifies (UpdateDisplay)

    %% Direct / Local UnityEvent Relationships
    PlayerController o-- AngelInteractor : Subscribes to onInteract
    PlayerController o-- gateOpener : Subscribes to onInteract
    PlayerController o-- CardinalLifter : Subscribes to onInteract
    StairSwitch ..> PlayerController : Detects & Ignores Collision
    PlayerDetector ..> PlayerController : Detects via Raycast

    %% Data & Config Dependencies
    MapRiddleController --> LevelConfigSO : Reads
    LevelTimer --> LevelConfigSO : Reads
    LevelTimer --> LevelData : Updates (isTimeUp)
    gateOpener --> LevelData : Updates (isGateOpen)
    CardinalLifter --> LevelData : Reads & Updates (isCardinalSafe)
    FallingDetector --> LevelData : Updates (isPlayerDead)
    showResults --> LevelData : Reads & Resets
```

### 2. פירוט קבוצות המחלקות והקשרים ביניהן

##### א. שכבת הנתונים וההגדרות (ScriptableObjects - Data & Config)

- מחלקת **`LevelConfigSO`**: מחלקת הגדרות סטטית המכילה את ערכי ברירת המחדל של השלב (זמן התחלתי, עונש זמן על בחירה שגויה במפה ועונש פגיעת אבן). נקראת על ידי `MapRiddleController` ו-`LevelTimer`.
    
- מחלקת **`LevelData`**: אובייקט נתונים משותף (Shared State) השומר את מצב התקדמות השחקן והשלב לאורך הסצנות (`isGateOpen`, `isCardinalSafe`, `isPlayerDead`, `isTimeUp`). משמש כגשר בין סצנת המשחק (`Level1Scene`) לבין סצנת הסיום (`GameResults`).
    

##### ב. מערכת האירועים המרכזית (Event Channels - Pub/Sub)

המערכת משתמשת בשני טיפוסי ערוצים (`VoidEventChannelSO` ו-`FloatEventChannelSO`) להפרדה מלאה בין השולחים למקבלים:

|שם הערוץ / האירוע|טיפוס ערוץ|מי שולח (Broadcaster)|מי מקבל (Listener)|מטרת האירוע|
|---|---|---|---|---|
|**`levelStartedChannel`**|`VoidEventChannelSO`|`MapRiddleController` (בבחירת מיקום נכון)|`PlayerController`, `LevelTimer`|פתיחת השליטה בתנועת השחקן והפעלת הספירה לאחור.|
|**`timePenaltyChannel` / `stoneHittedPlayer`**|`FloatEventChannelSO`|`MapRiddleController` (טעות במפה), `SpawnedStone` (פגיעה בשחקן)|`LevelTimer` (`ApplyPenalty`)|הפחתת שניות עונשין מהזמן הנותר בשעון.|
|**`timeUpdatedChannel`**|`FloatEventChannelSO`|`LevelTimer` (בכל פריים ובכל עונש)|`TimerUIView` (`UpdateDisplay`)|עדכון תצוגת הטיימר על המסך בפורמט `MM:SS`.|
|**`timeExpiredChannel`**|`VoidEventChannelSO`|`LevelTimer` (כשהזמן מגיע ל-`0`)|מאזיני מערכת / UI|התראה על פקיעת הזמן לפני מעבר לסצנת הסיום.|
|**`stoneDestroyedChannel`**|`VoidEventChannelSO`|`SpawnedStone` (בעת `OnDestroy`)|מנהל הספונר / מאזיני שלב|דיווח על השמדת אבן מתגלגלת (לצורך יצירת אבן חדשה או ניקוי).|

##### ג. אינטראקציות מקומיות בסצנה (Local UnityEvents & Physics)

- **`PlayerController`** $\leftrightarrow$ **`AngelInteractor` / `gateOpener` / `CardinalLifter`**:
    
    - `PlayerController` חושף אירוע מקומי מסוג `UnityEvent` בשם **`onInteract`**.
        
    - כאשר השחקן נכנס לאזור ה-Trigger של המלאך, השער או הקרדינל (`OnTriggerEnter2D`), הרכיב הרלוונטי נרשם דינמית (`AddListener`) לאירוע `onInteract` של השחקן, ומסיר את עצמו (`RemoveListener`) ביציאה מהאזור (`OnTriggerExit2D`).
        
    - לחיצה על מקש האינטראקציה מפעילה את המתודה הייעודית (`correctMirror`, `openGate`, או `LiftCardinal`).
        
- **`PlayerDetector`**:
    
    - מבצע בדיקת `Physics2D.Raycast` אנכית כלפי מטה. כאשר הקרן מזהה אובייקט בעל תגית `"Player"`, היא מפעילה את ה-`UnityEvent` הפנימי **`OnPlayerDetect`** (למשל לשחרור אבנים או הפעלת מלכודת).
        
- **`StairSwitch`**:
    
    - מזהה כניסת `PlayerController` ל-Trigger ברצפת הביניים ומשנה דינמית את התנגשות הפיזיקה (`Physics2D.IgnoreCollision`) מול המדרגות העליונות כדי לאפשר ירידה חלקה למפלס התחתון.
        
- **`FallingDetector` ו-`showResults`**:
    
    - `FallingDetector` מזהה נפילה לתהום, מעדכן `levelData.isPlayerDead = true` ומעביר לסצנת `GameResults`.
        
    - `showResults` קורא את הדגלים מתוך `LevelData` בסצנת הסיום כדי להציג הודעת ניצחון או הפסד מתאימה, ומאפס את הנתונים (`ResetData()`) בלחיצה על משחק חוזר.


# החלטות ארכיטקטורה

### 1. שימוש בערוצי אירועים מבוססי ScriptableObject (Event Channels) במקום הפניות ישירות (Direct References)
* **מה נבחר:** הפרדה מוחלטת (Decoupling) בין רכיבי המשחק השונים באמצעות נכסי `ScriptableObject` ייעודיים (`VoidEventChannelSO` ו-`FloatEventChannelSO`), המשמשים כמתווכים (Observer Pattern). לדוגמה: `MapRiddleController` ו-`SpawnedStone` משדרים קנסות זמן לערוץ `FloatEventChannelSO` (כמו `timePenaltyChannel` ו-`stoneHittedPlayer`), בעוד `LevelTimer` מאזין להן ומעדכן את `TimerUIView` דרך הערוץ `timeUpdatedChannel`. באופן דומה, פתיחת המשחק נעשית דרך `levelStartedChannel` שמפעיל במקביל את `PlayerController` ואת `LevelTimer`.
* **מה הייתה החלופה:** יצירת הפניות ישירות (Direct References / `GetComponent` / Singleton) בין המחלקות — למשל שהאבן הנופלת (`SpawnedStone`) או חידת המפה (`MapRiddleController`) יחזיקו הפניה ישירה ל-`LevelTimer`, וש-`LevelTimer` יחזיק הפניה ישירה לרכיב ה-UI (`TextMeshProUGUI`).
* **למה נבחרה גישה זו:** 
  1. **מניעת צימוד הדוק (Loose Coupling):** האבן, חידת המפה והטיימר אינם מכירים זה את זה ברמת הקוד. ניתן להחליף, להסיר או לשכפל אבנים ומלכודות מבלי לשבור את הטיימר או את ממשק המשתמש.
  2. **עבודה נוחה בסביבת Unity (Inspector-Driven):** חיבור השולחים והמאזינים מתבצע בגרירה ב-Inspector ללא תלות בהיררכיה של האובייקטים בסצנה (אין צורך שכל האובייקטים יהיו תחת אותו אב או באותה סצנה).
  3. **הרחבה פשוטה (Scalability):** הוספת מקור חדש להורדת זמן (כמו מלכודת נוספת) דורשת רק חיבור לאותו נכס ערוץ קיים, ללא שינוי שורת קוד אחת ב-`LevelTimer`.

---

### 2. שמירת מצב המשחק ומעבר נתונים בין סצנות באמצעות ScriptableObject (`LevelData` ו-`LevelConfigSO`)
* **מה נבחר:** הפרדת הנתונים וניהול מצב המשחק (State) מהאובייקטים שבסצנה אל תוך נכסי `ScriptableObject` חיצוניים: `LevelConfigSO` עבור פרמטרים קבועים של איזון המשחק (`startingTime`, `wrongMapPenalty`, `rockHitPenalty`), ו-`LevelData` עבור מצב הריצה הדינמי של השחקן והשלב (`isGateOpen`, `isCardinalSafe`, `isPlayerDead`, `isTimeUp`). מחלקות כמו `gateOpener`, `CardinalLifter`, `FallingDetector` ו-`LevelTimer` מעדכנות את `LevelData` בסצנת המשחק (`Level1Scene`), וסקריפט `showResults` קורא מתוכו את הנתונים בסצנת הסיום (`GameResults`) ומאפס אותו (`ResetData()`) בלחיצה על משחק חוזר.
* **מה הייתה החלופה:** שימוש באובייקט `GameManager` בשיטת **Singleton** עם `DontDestroyOnLoad` שנשאר חי בין סצנות, או שמירת הנתונים ב-`PlayerPrefs` / משתנים סטטיים (`static`).
* **למה נבחרה גישה זו:**
  1. **שמירה על עקרון האחריות היחידה (SRP):** כל רכיב בסצנה אחראי אך ורק על הלוגיקה שלו (פתיחת שער, זיהוי נפילה, הצגת תוצאות) ואינו מנהל בעצמו אובייקט גלובלי שחי לאורך כל מחזור החיים של האפליקציה.
  2. **מניעת בעיות אתחול וכפילויות במעבר סצנות:** בניגוד ל-`DontDestroyOnLoad` שעלול ליצור כפילויות של מנהלים בעת טעינה מחדש של `Level1Scene`, נכס `ScriptableObject` קיים כישות נתונים יחידה ברמת הפרויקט, נגיש ישירות מכל סצנה, וניתן לאיפוס מבוקר ומפורש (`levelData.ResetData()`).
  3. **איזון קל (Data-Driven Design):** ריכוז כל ערכי הקנסות והזמנים ב-`LevelConfigSO` מאפשר לכוונן את רמת הקושי של השלב מתוך ה-Inspector מבלי לגעת בקוד.

---

### 3. שילוב אירועי תגובה מקומיים (`UnityEvent` ו-Triggers) עבור אינטראקציות סביבתיות ממוקדות
* **מה נבחר:** שימוש במנגנון הרשמה דינמית לאירוע אינטראקציה מקומי (`public UnityEvent onInteract` ב-`PlayerController`) ובגלאי מבוסס קרן (`PlayerDetector` עם `Physics2D.Raycast` ו-`UnityEvent OnPlayerDetect`). כאשר השחקן נכנס לאזורי אינטראקציה נקודתיים (`AngelInteractor`, `gateOpener`, `CardinalLifter`), הם נרשמים זמנית ב-`OnTriggerEnter2D` אל `onInteract` של השחקן ומסירים את עצמם ב-`OnTriggerExit2D`. בנוסף, `StairSwitch` מבצע התאמה פיזיקלית מקומית (`Physics2D.IgnoreCollision`) רק בעת חציית רצפת הביניים.
* **מה הייתה החלופה:** ניהול מרכזי ב-`PlayerController` שבודק בכל לחיצת כפתור (`Interact`) מה נמצא מול השחקן באמצעות תגיות (`CompareTag`) או סדרת תנאי `if/else` (למשל: "אם לחצתי על כפתור ליד מלאך תסובב מראה, אם ליד שער תפתח שער, אם ליד קרדינל תרים אותו").
* **למה נבחרה גישה זו:**
  1. **עמידה בעקרון הפתוח-סגור (Open/Closed Principle):** מחלקת `PlayerController` נשארת נקייה לחלוטין מלוגיקה של חידות או חפצים בעולם — היא רק מודיעה "השחקן ביצע אינטראקציה" (`onInteract.Invoke()`), וכל אובייקט בסביבה אחראי על ההתנהגות שלו כשהשחקן בקרבתו.
  2. **ביצועים ודיוק:** במקום לבצע בדיקות רציפות כל פריים מתוך השחקן, הרישום מתבצע רק בטווח הרלוונטי, ושימוש ב-`Physics2D.Raycast` בתוך `PlayerDetector` מאפשר זיהוי מדויק של חציית קו הראייה/מלכודת ללא תלות בצורת הקוליידר של השחקן.

# איך מוסיפים סוג אויב או חפץ חדש: אילו קבצים משתנים
המחלקה StoneSpawner - אם נרצה להשתמש בקובץ בשביל ליצור אינסטנסים מסוג אחר פשוט נגרור בחלון הINSPECTOR של האובייקט שמכיל את הסקריפט, את ה PREFAB הרלוונטי 
לשדה ה prefabToSpawn

