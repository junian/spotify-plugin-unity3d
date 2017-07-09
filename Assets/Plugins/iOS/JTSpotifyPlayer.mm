//
//  JTSpotifyPlayer.m
//  cobaspotify
//
//  Created by Junian on 7/9/17.
//  Copyright © 2017 Junian.Net. All rights reserved.
//

#import "JTSpotifyPlayer.h"
#include "AppDelegateListener.h"

@interface JTSpotifyPlayer ()

@property (nonatomic, strong) SPTAuth *auth;
@property (nonatomic, strong) SPTAudioStreamingController *player;
@property (nonatomic, strong) UIViewController *authViewController;

@end

@implementation JTSpotifyPlayer

+ (id)instance {
    static JTSpotifyPlayer *sharedMyManager = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedMyManager = [[self alloc] init];
    });
    return sharedMyManager;
}

- (id) init {
    if (self = [super init]) {
        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(onOpenURL:)
                                                     name:kUnityOnOpenURL object:nil];
    }
    return self;
}

- (void) onOpenURL:(NSNotification*)notification {
    NSURL* url = [[notification userInfo] objectForKey:@"url"];
    [self handleURL:url];
}

- (void)dealloc {
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

- (void) initWithClientID: (NSString*) clientID
           andCallbackUrl:(NSString*) callbackUrl
{
    self.auth = [SPTAuth defaultInstance];
    self.player = [SPTAudioStreamingController sharedInstance];
    
    // The client ID you got from the developer site
    self.auth.clientID = clientID;
    // The redirect URL as you entered it at the developer site
    self.auth.redirectURL = [NSURL URLWithString:callbackUrl];
    // Setting the `sessionUserDefaultsKey` enables SPTAuth to automatically store the session object for future use.
    self.auth.sessionUserDefaultsKey = @"current session";
    // Set the scopes you need the user to authorize. `SPTAuthStreamingScope` is required for playing audio.
    self.auth.requestedScopes = @[SPTAuthStreamingScope];
    
    // Become the streaming controller delegate
    self.player.delegate = self;
    
    // Start up the streaming controller.
    NSError *audioStreamingInitError;
    NSAssert([self.player startWithClientId:self.auth.clientID error:&audioStreamingInitError],
             @"There was a problem starting the Spotify SDK: %@", audioStreamingInitError.description);
    
    // Start authenticating when the app is finished launching
    dispatch_async(dispatch_get_main_queue(), ^{
        [self startAuthenticationFlow];
    });
}

- (void)startAuthenticationFlow
{
    // Check if we could use the access token we already have
    if ([self.auth.session isValid]) {
        // Use it to log in
        [self.player loginWithAccessToken:self.auth.session.accessToken];
    } else {
        // Get the URL to the Spotify authorization portal
        NSURL *authURL = [self.auth spotifyWebAuthenticationURL];
        // Present in a SafariViewController
        self.authViewController = [[SFSafariViewController alloc] initWithURL:authURL];
        [UnityGetMainWindow().rootViewController presentViewController:self.authViewController animated:YES completion:nil];
    }
}

- (BOOL) handleURL:(NSURL *)url
{
    // If the incoming url is what we expect we handle it
    if ([self.auth canHandleURL:url]) {
        // Close the authentication window
        [self.authViewController.presentingViewController dismissViewControllerAnimated:YES completion:nil];
        self.authViewController = nil;
        // Parse the incoming url to a session object
        [self.auth handleAuthCallbackWithTriggeredAuthURL:url callback:^(NSError *error, SPTSession *session) {
            if (session) {
                // login to the player
                [self.player loginWithAccessToken:self.auth.session.accessToken];
            }
        }];
        return YES;
    }
    return NO;
}

- (void)audioStreamingDidLogin:(SPTAudioStreamingController *)audioStreaming
{
    [self.player playSpotifyURI:@"spotify:track:58s6EuEYJdlb0kO7awm3Vp" startingWithIndex:0 startingWithPosition:0 callback:^(NSError *error) {
        if (error != nil) {
            NSLog(@"*** failed to play: %@", error);
            return;
        }
    }];
}

@end

// Converts C style string to NSString
NSString* CreateNSString (const char* string)
{
    if (string)
        return [NSString stringWithUTF8String: string];
    else
        return [NSString stringWithUTF8String: ""];
}

// Helper method to create C string copy
char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

// When native code plugin is implemented in .mm / .cpp file, then functions
// should be surrounded with extern "C" block to conform C function naming rules
extern "C" {
    
    void _LogToiOS(const char* debugMessage) {
        NSLog(@"UniPlugins log %@", [NSString stringWithUTF8String:debugMessage]);
    }
    
    void _Init(const char* clientID, const char* callbackUrl)
    {
        [[JTSpotifyPlayer instance] initWithClientID:CreateNSString(clientID) andCallbackUrl:CreateNSString(callbackUrl)];
        
        _LogToiOS(clientID);
    }
    
}
