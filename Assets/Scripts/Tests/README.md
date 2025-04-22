# Testing Enemy and Level Data Loading

This directory contains scripts to test if enemy and level data is being loaded correctly from JSON files.

## How to Test

1. Create a new scene called "DataTest"
2. Add an empty GameObject to the scene
3. Add the `DataTester` component to this GameObject
4. Run the scene
5. Check the Unity Console for test results

## Expected Results

If enemy data is loading correctly, you should see:
- A message showing the total number of enemies loaded
- Details for each enemy (zombie, skeleton, warlock)
- Confirmation that specific enemies can be accessed by name

If level data is loading correctly, you should see:
- A message showing the total number of levels loaded
- Details for each level (Easy, Medium, Endless)
- Details of each spawn configuration within each level
- Confirmation that specific levels can be accessed by name

## Troubleshooting

If you don't see the expected results:

1. Make sure the JSON files (`enemies.json` and `levels.json`) are in the Resources folder
2. Check that the JSON files have the correct format
3. Verify that Newtonsoft.Json is properly included in your project
4. Check for any errors in the console related to deserialization
5. Ensure the classes `Enemy`, `Level`, and `Spawn` have fields matching the JSON structure 