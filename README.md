# temp

Small Laravel REST API that stores per-player waste data as JSON, plus a folder of Unity C# scripts that call it. The Unity side is scripts only, not a full Unity project.

## What is in it

`app/`, `routes/`, `config/` and the rest of the Laravel tree are the standard Laravel 12 skeleton with one addition, `PlayerWasteController`, which backs four routes under `/api/v1`. The welcome page in `resources/views` is still the default.

`Unity/` has seven `.cs` files and nothing else: no `Assets/`, `ProjectSettings/`, scenes or prefabs, so the Unity version is not recorded anywhere and the scripts cannot be opened as a project as they are.

## Stack

- PHP ^8.2, Laravel ^12.0, Laravel Sanctum ^4.0 (installed, but no route uses it)
- PHPUnit ^11.5 (only the two default example tests)
- Vite 6 and Tailwind 4 in `package.json`, used only by the default welcome page
- Unity scripts using `UnityEngine.Networking` and `JsonUtility`

Checked with PHP 8.3.

## Running the API

```
composer install
cp .env.example .env
php artisan key:generate
php artisan serve
```

The API is served at `http://127.0.0.1:8000/api/v1`, which is the address hard-coded in the Unity scripts. The waste routes read and write a JSON file and do not use the database. `.env.example` selects SQLite for sessions, cache and queue, which only matters for the default Laravel pages and the queue/cache tables.

```
php artisan test
```

## API

| Method | Path | Body | Result |
| --- | --- | --- | --- |
| GET | `/waste/{player_id}` | | the player, or 404 |
| POST | `/waste` | `player_id` (string), `waste_quants` (number), `rat_count` (integer) | 201 with the record, 400 if the id exists |
| PUT | `/waste/{player_id}` | `waste_quants` and/or `rat_count` | the updated record, or 404 |
| DELETE | `/waste/{player_id}` | | 204, or 404 |

Send `Accept: application/json` to get JSON validation errors. Records look like `{"player_id": "p1", "waste_quants": 3.5, "rat_count": 1}`.

Storage details: the controller checks for `player_waste.json` with the `Storage` facade (which resolves to `storage/app/private/` in Laravel 12) but reads and writes `storage/app/player_waste.json` directly. The data file is therefore created at `storage/app/player_waste.json` on the first POST, and a placeholder file with an empty players list is created in `storage/app/private/`. Neither file is committed.

## Unity scripts

| File | What it does |
| --- | --- |
| `ApiClient.cs` | Thin wrapper over `UnityWebRequest` with coroutine `Get`, `Post`, `Put` and `Delete` against the base URL. |
| `Player.cs` | Serializable class with `player_id`, `waste_quants` and `rat_count`, matching the API fields. |
| `GameManager.cs` | Singleton. On start it loads `player123` from the API and creates it if the request fails. `UpdateWaste(amount)` adds to `waste_quants` and PUTs the player. |
| `Engine.prefab.cs` | `Engine` component. Spawns a player prefab and up to 5 chef prefabs (random position within 10 units on x and z), loads or creates `player123`, has a Player ID input and select button, and lowers `waste_quants` while the player is within 2 units of a chef. |
| `ChefAI.cs` | Chefs patrol back and forth on the x axis (`patrolRange` 5, `patrolSpeed` 2). Within 2 units of the player they call `GameManager.UpdateWaste(-0.1f)`. |
| `PlayerMovement.cs` | Moves the player on the x/z plane with the `Horizontal` and `Vertical` axes (WASD or arrow keys with Unity's default input settings), `moveSpeed` 5. |
| `CharacterSelection.cs` | UI script: type a player id, press the button, load that player from the API and store it in `GameManager`. Also has a `CreateNewPlayer` method. |

So the only mechanic the scripts show is waste going down while the player stands near a chef, with the value saved to the backend. `rat_count` is stored and sent but nothing in the scripts changes it.

## Known gaps

- Unity project files are missing, so the scripts have never been verified here.
- `CharacterSelection.cs` assigns the private `GameManager.currentPlayer` and calls the private `GameManager.CreatePlayer`, which would not compile.
- `Engine.prefab.cs` declares class `Engine`; Unity expects the file name to match the class name for components.
- `Engine` and `GameManager` both load or create `player123` and both implement waste updates, so they overlap.
- `ChefAI` and `Engine.ManageWaste` send a PUT every frame while the player is near a chef. `GameManager.UpdateWaste` does not clamp at zero.
- `Engine.OnDestroy` calls `RemovePlayer`, which deletes the player's record from the API.
- No authentication on the API, and no locking on the JSON file.
- The Laravel workflows in `.github/workflows` trigger on `master` and `*.x` branches; the default branch here is `main`.
