#import <UIKit/UIKit.h>

@interface FileOpener : NSObject <UIDocumentInteractionControllerDelegate>
@property (strong, nonatomic) UIDocumentInteractionController *documentInteractionController;
@end

@implementation FileOpener

- (id)init {
    self = [super init];
    return self;
}

// Implement the delegate method
- (UIViewController *)documentInteractionControllerViewControllerForPreview:(UIDocumentInteractionController *)controller {
    // You should return the top-most view controller of the app
    return [UIApplication sharedApplication].keyWindow.rootViewController;
}

- (void)openFileAtPath:(NSString *)path {
    // Remove any '/file:' if it exists in the path string
    NSString *correctedPath = [path stringByReplacingOccurrencesOfString:@"/file:" withString:@""];
    
    dispatch_async(dispatch_get_main_queue(), ^{
        NSURL *fileURL = [NSURL fileURLWithPath:correctedPath];
        self.documentInteractionController = [UIDocumentInteractionController interactionControllerWithURL:fileURL];
        self.documentInteractionController.delegate = self;
        
        UIViewController *presentingVC = [UIApplication sharedApplication].keyWindow.rootViewController;
        while (presentingVC.presentedViewController) {
            presentingVC = presentingVC.presentedViewController;
        }
        
        if (presentingVC.isViewLoaded && presentingVC.view.window) {
            [self.documentInteractionController presentPreviewAnimated:YES];
        } else {
            NSLog(@"The view controller is not ready for presentation.");
        }
    });
}


@end

// NOTE: parts below here are coming from the example under https://docs.unity3d.com/uploads/Examples/iPhoneNativeCodeSample.zip

static FileOpener* fileOpener = nil;

// Converts C style string to NSString
NSString* CreateNSString (const char* string) {
    if (string) {
        return [NSString stringWithUTF8String: string];
    } else {
        return [NSString stringWithUTF8String: ""];
    }
}

// When native code plugin is implemented in .mm / .cpp file, then functions
// should be surrounded with extern "C" block to conform C function naming rules
extern "C" {
    void _OpenFile(const char* path) {
        if (fileOpener == nil) {
            fileOpener = [[FileOpener alloc] init];
        }
        [fileOpener openFileAtPath: CreateNSString(path)];
    }
}
