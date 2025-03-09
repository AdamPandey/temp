<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\Storage;

class PlayerWasteController extends Controller
{
    private function getJsonData()
    {
        $path = storage_path('app/player_waste.json');

        
        if (!Storage::exists('player_waste.json')) {
            Storage::put('player_waste.json', json_encode(['players' => []]));
        }

        
        if (!is_readable($path)) {
            \Log::error("File not readable: " . $path);
            return ['players' => []];
        }

        return json_decode(file_get_contents($path), true) ?? ['players' => []];
    }


    private function saveJsonData($data)
    {
        $path = storage_path('app/player_waste.json');
        $json = json_encode($data, JSON_PRETTY_PRINT);

        \Log::info('Saving to: ' . $path);
        \Log::info('Data: ' . $json);

        file_put_contents($path, $json);
    }

    public function show($player_id)
    {
        $data = $this->getJsonData();
        $player = collect($data['players'])->firstWhere('player_id', $player_id);
        if (!$player) return response()->json(['error' => 'Player not found'], 404);
        return response()->json($player);
    }

    public function store(Request $request)
    {
    $validated = $request->validate([
        'player_id' => 'required|string',
        'waste_quants' => 'required|numeric',
        'rat_count' => 'required|integer',
    ]);
    $data = $this->getJsonData();
    if (collect($data['players'])->contains('player_id', $validated['player_id'])) {
        return response()->json(['error' => 'Player ID already exists'], 400);
    }
    $data['players'][] = $validated; // This should add the new player
    $this->saveJsonData($data);
    return response()->json($validated, 201);
    }

    public function update(Request $request, $player_id)
    {
        $data = $this->getJsonData();
        $index = collect($data['players'])->search(function ($item) use ($player_id) {
            return $item['player_id'] === $player_id;
        });
        if ($index === false) return response()->json(['error' => 'Player not found'], 404);
        $validated = $request->validate(['waste_quants' => 'numeric', 'rat_count' => 'integer']);
        $data['players'][$index] = array_merge($data['players'][$index], $validated);
        $this->saveJsonData($data);
        return response()->json($data['players'][$index]);
    }

    public function destroy($player_id)
    {
        $data = $this->getJsonData();
        $index = collect($data['players'])->search(function ($item) use ($player_id) {
            return $item['player_id'] === $player_id;
        });
        if ($index === false) return response()->json(['error' => 'Player not found'], 404);
        array_splice($data['players'], $index, 1);
        $this->saveJsonData($data);
        return response()->json(null, 204);
    }
}