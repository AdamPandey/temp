<?php

use App\Http\Controllers\PlayerWasteController;
use Illuminate\Support\Facades\Route;

Route::prefix('v1')->group(function () {
    Route::get('/waste/{player_id}', [PlayerWasteController::class, 'show']);
    Route::post('/waste', [PlayerWasteController::class, 'store']);
    Route::put('/waste/{player_id}', [PlayerWasteController::class, 'update']);
    Route::delete('/waste/{player_id}', [PlayerWasteController::class, 'destroy']);
});