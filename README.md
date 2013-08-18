CameraAndResolutionBuddiesSample
================================

custom resolution with a camera to keep everything on screen

This game shows how to tie together a bunch of different code modules:
CameraBuddy
ResolutionBuddy
CollisionBuddy

This game has a background image that takes up the whole screen.  We don't want to show anything outside that image, so we use the WorldBoundary piece of the CameraBuddy to restrict the camera.  There are a two circles, the player controls the green one with the arrow keys or controller. The camera will pan & zoom to keep both circles on screen.  If the circles collide, the screen will shake like it was a big crash.