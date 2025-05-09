#import <Foundation/Foundation.h>

extern "C" {
    const char* _getCFBundleVersion() {
        NSString *build = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleVersion"];
        return [build UTF8String];
    }
}
