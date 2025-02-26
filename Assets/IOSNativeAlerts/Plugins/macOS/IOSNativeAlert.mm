#import <Cocoa/Cocoa.h>

extern "C" {
    void ShowMacOSAlert(const char* title, const char* message, const char* buttons[], int buttonCount) {
        @autoreleasepool {
            NSAlert *alert = [[NSAlert alloc] init];
            alert.messageText = [NSString stringWithUTF8String:title];
            alert.informativeText = [NSString stringWithUTF8String:message];

            // Add buttons
            for (int i = 0; i < buttonCount; i++) {
                NSString *buttonTitle = [NSString stringWithUTF8String:buttons[i]];
                [alert addButtonWithTitle:buttonTitle];
            }

            // Run alert in main thread (important for UI actions)
            dispatch_async(dispatch_get_main_queue(), ^{
                [alert runModal];
            });
        }
    }
}
