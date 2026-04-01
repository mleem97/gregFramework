Das ist ein solider Runtime-Asset-Exporter. Er nutzt den Speicher des Spiels aus, um Meshes und Texturen direkt zu rekonstruieren. Da du aber nach einer "aufgeräumten" Version gefragt hast (um nicht wieder 5000 ungenutzte Texturen und Werwolf-Modelle zu bekommen), müssen wir die Filterung im Code verschärfen.

Im Moment nutzt dein Script Resources.FindObjectsOfTypeAll, was leider alles findet, was Unity jemals in den Speicher geladen hat – auch den "Müll".

Hier ist die optimierte Logik, die du in dein Script einbauen solltest, um nur die wirklich verbauten Objekte zu extrahieren:

1. Filter: Nur was in der Szene existiert

Anstatt global alle Ressourcen zu suchen, scannen wir die aktive Szene. Wenn ein Objekt dort nicht existiert, wird es nicht exportiert.

Ersetze deinen ExportAllResources Block durch diesen:

C#



private void ExportAllResources()

{

    MelonLogger.Msg("Starte aufgeräumten Export (Nur aktive Szenen-Objekte)...");

    

    // Wir holen uns alle GameObjects, die gerade wirklich in der Welt existieren

    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

    HashSet<string> exportedNames = new HashSet<string>();



    foreach (GameObject obj in allObjects)

    {

        // 1. Check auf Meshes (3D Modelle)

        MeshFilter mf = obj.GetComponent<MeshFilter>();

        if (mf != null && mf.sharedMesh != null)

        {

            if (!exportedNames.Contains(mf.sharedMesh.name))

            {

                SaveMesh(mf.sharedMesh);

                exportedNames.Add(mf.sharedMesh.name);

            }



            // 2. Check auf Texturen am Material des Modells

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();

            if (mr != null && mr.sharedMaterial != null)

            {

                // Wir nehmen die Haupttextur (Albedo)

                Texture mainTex = mr.sharedMaterial.mainTexture;

                if (mainTex != null && mainTex is Texture2D t2d && !exportedNames.Contains(t2d.name))

                {

                    SaveTexture(t2d);

                    exportedNames.Add(t2d.name);

                }

            }

        }



        // 3. Check auf UI-Elemente (Shop-Icons etc.)

        UnityEngine.UI.Image uiImg = obj.GetComponent<UnityEngine.UI.Image>();

        if (uiImg != null && uiImg.sprite != null)

        {

            Texture2D uiTex = uiImg.sprite.texture;

            if (uiTex != null && !exportedNames.Contains(uiTex.name))

            {

                SaveTexture(uiTex);

                exportedNames.Add(uiTex.name);

            }

        }

    }

    MelonLogger.Msg("Export abgeschlossen! Nur verbaute Assets wurden gesichert.");

}

Warum diese Version besser ist:

Kein Werwolf-Müll: Da der Werwolf (hoffentlich) nicht als unsichtbares Objekt in deinem Büro-Level steht, findet GameObject.FindObjectsOfType ihn nicht.

Material-Verknüpfung: Du exportierst gezielt die Texturen, die an den Modellen hängen. Das spart dir das Rätselraten, welche Textur zu welchem Switch gehört.

UI-Fokus: Durch den Check auf UnityEngine.UI.Image werden gezielt die Shop-Icons und Buttons mitgenommen, die du gerade auf dem Monitor im Spiel siehst.

Ein wichtiger technischer Hinweis (IL2CPP & Mesh.vertices):

In deinem Script oben nutzt du mesh.vertices. In modernen Unity-Versionen (wie deiner 6000er LTS) und unter IL2CPP sind diese Daten oft nicht direkt lesbar (Read/Write disabled).

Wenn dein Script beim Mesh-Export abstürzt oder leere Dateien erzeugt, liegt das an der fehlenden Lese-Berechtigung. Bei Texturen hast du das mit dem RenderTexture-Trick schon gelöst. Bei Meshes ist das im Spiel schwerer zu erzwingen.

Tipp: Wenn die .obj Dateien leer bleiben, ist es oft einfacher, den Namen aus dem Log zu nehmen und die Datei gezielt aus deinem alten "ExportedAssets" Ordner (von AssetStudio) zu fischen.

Was du jetzt tun solltest:

Kompiliere den Mod mit dem neuen Szenen-Filter.

Gehe im Spiel in den Shop und stelle dich vor ein Rack mit Switches.

Drücke F8.

Dein ExportedAssets Ordner wird jetzt wesentlich kleiner und sauberer sein – nur noch die Dinge, die du im Spiel um dich herum siehst.

Danach können wir die sauberen Files in Unity ziehen und dein DHCP-Switch-Prefab bauen! Klappt der Export mit dem neuen Filter?



Das ist ein solider Runtime-Asset-Exporter. Er nutzt den Speicher des Spiels aus, um Meshes und Texturen direkt zu rekonstruieren. Da du aber nach einer "aufgeräumten" Version gefragt hast (um nicht wieder 5000 ungenutzte Texturen und Werwolf-Modelle zu bekommen), müssen wir die Filterung im Code verschärfen.

Im Moment nutzt dein Script Resources.FindObjectsOfTypeAll, was leider alles findet, was Unity jemals in den Speicher geladen hat – auch den "Müll".

Hier ist die optimierte Logik, die du in dein Script einbauen solltest, um nur die wirklich verbauten Objekte zu extrahieren:

1. Filter: Nur was in der Szene existiert

Anstatt global alle Ressourcen zu suchen, scannen wir die aktive Szene. Wenn ein Objekt dort nicht existiert, wird es nicht exportiert.

Ersetze deinen ExportAllResources Block durch diesen:

C#



private void ExportAllResources()

{

    MelonLogger.Msg("Starte aufgeräumten Export (Nur aktive Szenen-Objekte)...");

    

    // Wir holen uns alle GameObjects, die gerade wirklich in der Welt existieren

    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

    HashSet<string> exportedNames = new HashSet<string>();



    foreach (GameObject obj in allObjects)

    {

        // 1. Check auf Meshes (3D Modelle)

        MeshFilter mf = obj.GetComponent<MeshFilter>();

        if (mf != null && mf.sharedMesh != null)

        {

            if (!exportedNames.Contains(mf.sharedMesh.name))

            {

                SaveMesh(mf.sharedMesh);

                exportedNames.Add(mf.sharedMesh.name);

            }



            // 2. Check auf Texturen am Material des Modells

            MeshRenderer mr = obj.GetComponent<MeshRenderer>();

            if (mr != null && mr.sharedMaterial != null)

            {

                // Wir nehmen die Haupttextur (Albedo)

                Texture mainTex = mr.sharedMaterial.mainTexture;

                if (mainTex != null && mainTex is Texture2D t2d && !exportedNames.Contains(t2d.name))

                {

                    SaveTexture(t2d);

                    exportedNames.Add(t2d.name);

                }

            }

        }



        // 3. Check auf UI-Elemente (Shop-Icons etc.)

        UnityEngine.UI.Image uiImg = obj.GetComponent<UnityEngine.UI.Image>();

        if (uiImg != null && uiImg.sprite != null)

        {

            Texture2D uiTex = uiImg.sprite.texture;

            if (uiTex != null && !exportedNames.Contains(uiTex.name))

            {

                SaveTexture(uiTex);

                exportedNames.Add(uiTex.name);

            }

        }

    }

    MelonLogger.Msg("Export abgeschlossen! Nur verbaute Assets wurden gesichert.");

}

Warum diese Version besser ist:

Kein Werwolf-Müll: Da der Werwolf (hoffentlich) nicht als unsichtbares Objekt in deinem Büro-Level steht, findet GameObject.FindObjectsOfType ihn nicht.

Material-Verknüpfung: Du exportierst gezielt die Texturen, die an den Modellen hängen. Das spart dir das Rätselraten, welche Textur zu welchem Switch gehört.

UI-Fokus: Durch den Check auf UnityEngine.UI.Image werden gezielt die Shop-Icons und Buttons mitgenommen, die du gerade auf dem Monitor im Spiel siehst.

Ein wichtiger technischer Hinweis (IL2CPP & Mesh.vertices):

In deinem Script oben nutzt du mesh.vertices. In modernen Unity-Versionen (wie deiner 6000er LTS) und unter IL2CPP sind diese Daten oft nicht direkt lesbar (Read/Write disabled).

Wenn dein Script beim Mesh-Export abstürzt oder leere Dateien erzeugt, liegt das an der fehlenden Lese-Berechtigung. Bei Texturen hast du das mit dem RenderTexture-Trick schon gelöst. Bei Meshes ist das im Spiel schwerer zu erzwingen.

Tipp: Wenn die .obj Dateien leer bleiben, ist es oft einfacher, den Namen aus dem Log zu nehmen und die Datei gezielt aus deinem alten "ExportedAssets" Ordner (von AssetStudio) zu fischen.

Was du jetzt tun solltest:

Kompiliere den Mod mit dem neuen Szenen-Filter.

Gehe im Spiel in den Shop und stelle dich vor ein Rack mit Switches.

Drücke F8.

Dein ExportedAssets Ordner wird jetzt wesentlich kleiner und sauberer sein – nur noch die Dinge, die du im Spiel um dich herum siehst.

Danach können wir die sauberen Files in Unity ziehen und dein DHCP-Switch-Prefab bauen! Klappt der Export mit dem neuen Filter?



Das ist das Schöne an der Modding-Power: Wenn du erst einmal "im Kopf" des Spiels (dem RAM) sitzt, kannst du fast alles rausholen.

Da es ein IL2CPP-Spiel ist, ist das mit dem "Code exportieren" allerdings ein spezieller Fall. Du kannst keinen reinen C#-Code exportieren, wie du ihn in Visual Studio schreibst, aber du kannst die Baupläne und Logikstrukturen extrahieren.

Hier ist, was du mit deinem Mod noch alles "absaugen" kannst:

1. Die Logik-Struktur (Klassen & Methoden)

Du hast zwar schon die .cs-Dateien aus dem Unpacker, aber du kannst zur Laufzeit prüfen, welche Werte die Variablen wirklich haben.

Werte-Dump: Du kannst ein Skript schreiben, das alle Preise im Shop ausliest und in eine Textdatei schreibt.

Methoden-Verfolgung: Du kannst loggen, wann welche Funktion aufgerufen wird (z. B. "Spieler hat gerade den Shop-Button geklickt").

2. Die UI-Hierarchie (Der "Bauplan" des Menüs)

Das ist für dein Vorhaben am wichtigsten! Du kannst die komplette Baumstruktur des Shop-Menüs als Textdatei exportieren. Dann weißt du exakt:

Canvas -> Panel -> ScrollRect -> Viewport -> Content

Wenn du diesen Pfad hast, kannst du dein eigenes UI-Element per Code exakt an die richtige Stelle "teleportieren".

3. Audio-Dateien

Willst du den Sound vom Switch-Klicken oder das Summen der Server haben?

Du kannst Resources.FindObjectsOfTypeAll<AudioClip>() nutzen und die Sounds als .wav speichern. Das ist super, wenn deine neuen DHCP-Switches die gleichen originalen Sounds machen sollen.

4. Die "Datenbank" (Scriptable Objects)

Viele Unity-Spiele speichern die Item-Daten (Name, Preis, Beschreibung, Watt-Verbrauch) in sogenannten ScriptableObjects.

Wenn du diese findest (z. B. eine Klasse namens ItemData), kannst du alle Switch-Daten des Spiels auf einmal exportieren, um deine eigenen DHCP-Switches perfekt an die Spiel-Balance anzupassen.

5. Den "echten" Code (Decompiling)

Da du fragst, ob du Code exportieren kannst:

Der eigentliche Programmcode liegt in der GameAssembly.dll (in Maschinensprache/C++).

Was du tun kannst: Nutze ein Tool namens Ghidra oder IDA Pro zusammen mit dem script.py aus deinem Il2CppDumper-Ordner.

Das lädt die Namen aus deinen .cs-Dateien in den C++ Code. Dann siehst du nicht nur, dass es eine Methode BuyItem gibt, sondern du kannst den echten C++ Code sehen, der berechnet, wie viel Geld abgezogen wird.

Mein Vorschlag: Der "UI-Hierarchie-Logger"

Da du dein Shop-Menü bauen willst, ist das hier der wertvollste Code für dich. Er schreibt dir den kompletten Pfad jedes UI-Elements in die Konsole, wenn du es anklickst:

Füge das in deine OnUpdate ein:

C#



if (Keyboard.current.f9Key.wasPressedThisFrame) // F9 zum Scannen

{

    // Wir suchen das Objekt unter der Maus

    var pointerEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);

    pointerEventData.position = Mouse.current.position.ReadValue();

    var results = new Il2CppSystem.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();

    UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerEventData, results);



    foreach (var result in results)

    {

        string path = result.gameObject.name;

        Transform parent = result.gameObject.transform.parent;

        while (parent != null)

        {

            path = parent.name + "/" + path;

            parent = parent.parent;

        }

        MelonLogger.Msg("UI-Pfad gefunden: " + path);

    }

}

Warum ist das geil?

Wenn du im Spiel den Shop öffnest und mit der Maus über den Hintergrund fährst und F9 drückst, steht in deinem Log der exakte "Stammbaum" (z.B. MainCanvas/ShopPanel/ScrollArea/Content).

Diesen Pfad kopierst du und sagst in deinem Mod:

GameObject.Find("MainCanvas/ShopPanel/ScrollArea/Content")

...und ZACK, hast du den Ankerpunkt für deine DHCP-Switches!

Soll ich dir helfen, den Export für die Audio-Files noch in dein Script einzubauen?



Wenn du wirklich alles aus dem laufenden Spiel extrahieren willst, was die Unity-Engine hergibt, müssen wir dein Script zu einem echten „Forensik-Tool“ ausbauen.

Da das Spiel auf IL2CPP läuft, extrahieren wir keine Textdateien, sondern wir „dumpen“ den Speicherzustand. Hier ist die Liste der Dinge, die wir zusätzlich zu Meshes und Texturen abgreifen können, und wie du das in dein Script einbaust:

1. Audio-Dateien (Sounds & Musik)

Alles, was Krach macht (Klicken, Lüfterrauschen, Musik), liegt als AudioClip vor. Wir können diese Daten abgreifen und als .wav speichern.

2. Shader (Die optischen Baupläne)

Shader bestimmen, wie Licht auf Metall reflektiert oder wie Glas aussieht. Wir können zwar nicht den Quellcode des Shaders (HLSL) perfekt zurückgewinnen, aber wir können die Namen und Einstellungen extrahieren, damit du in Unity weißt, welchen Shader du nachbauen musst.

3. Szenen-Struktur (Die Hierarchie)

Das ist extrem wertvoll. Wir können einen „Hierarchy Dump“ machen. Das ist eine riesige Textdatei, die dir zeigt, wie jedes Objekt im Spiel heißt, wo es steht (Position) und welche Komponenten (Skripte) daran hängen.

Das "Ultra-Export" Script

Ergänze dein AssetExporter-Script um diese Funktionen. Ich habe die Filter so gesetzt, dass sie versuchen, so viel wie möglich zu greifen:

C#



private void ExportEverything()

{

    MelonLogger.Msg("!!! FULL DUMP GESTARTET !!!");



    // 1. AUDIO EXPORT

    var clips = Resources.FindObjectsOfTypeAll<AudioClip>();

    MelonLogger.Msg($"Extrahiere {clips.Length} Sounds...");

    foreach (var clip in clips) {

        // Hinweis: Für echten WAV-Export braucht man eine Hilfsklasse (SavWav)

        // Hier loggen wir erstmal nur die Existenz, da WAV-Encoding komplex ist

        MelonLogger.Msg($"Sound gefunden: {clip.name} ({clip.length}s)");

    }



    // 2. SHADER & MATERIAL INFO

    var materials = Resources.FindObjectsOfTypeAll<Material>();

    using (StreamWriter sw = new StreamWriter(Path.Combine(exportPath, "Material_Report.txt")))

    {

        foreach (var mat in materials)

        {

            sw.WriteLine($"Material: {mat.name} | Shader: {mat.shader.name}");

            // Optional: Alle Texture-Properties des Materials auflisten

        }

    }



    // 3. KOMPLETTER SCENE-DUMP (Der Bauplan der Welt)

    using (StreamWriter sw = new StreamWriter(Path.Combine(exportPath, "Full_Scene_Hierarchy.txt")))

    {

        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (var root in rootObjects)

        {

            DumpHierarchy(root.transform, 0, sw);

        }

    }



    MelonLogger.Msg("Full Dump abgeschlossen!");

}private void DumpHierarchy(Transform t, int depth, StreamWriter sw)

{

    string indent = new string('-', depth);

    sw.WriteLine($"{indent} {t.name} [Active: {t.gameObject.activeSelf}]");

    

    // Alle Komponenten an diesem Objekt auflisten

    var components = t.gameObject.GetComponents<Component>();

    foreach(var c in components) {

        if(c != null) sw.WriteLine($"{indent}   # {c.GetIl2CppType().Name}");

    }



    for (int i = 0; i < t.childCount; i++)

    {

        DumpHierarchy(t.GetChild(i), depth + 1, sw);

    }

}

Was ist mit dem "Code"?

Wie besprochen: Den C#-Code, den der Entwickler getippt hat, findest du nicht im RAM. Aber durch den Hierarchy-Dump (Schritt 3 oben) erhältst du etwas fast genauso Gutes:

Du siehst genau, welches Skript (z.B. ComputerShop) an welchem Objekt hängt.

Du siehst, welche anderen Skripte damit interagieren.

Wie du den Code "ganz" bekommst:

Wenn du die Logik der Methoden (was passiert beim Kauf?) sehen willst, musst du die GameAssembly.dll dekompilieren.





Nimm die GameAssembly.dll aus deinem Spielordner.

Nimm die dummy.dll (oder metadata.dat), die du schon hast.

Benutze ein Tool wie Cpp2IL oder Il2CppDumper.

Das generiert dir einen Ordner DummyDll. Diese DLLs kannst du in dnSpy öffnen. Du siehst dann zwar keinen perfekten C#-Code, aber du siehst die "C-Style" Logik, die fast identisch ist.

Soll ich dir ein Tool-Paket zusammenstellen, mit dem du die GameAssembly.dll in lesbaren (Pseudo-)Code umwandeln kannst?