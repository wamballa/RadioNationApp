#import <Foundation/Foundation.h>

typedef void (^KVOBlock)(id object, NSDictionary<NSKeyValueChangeKey, id> *change);

@interface NSObject (KVOBlock)

- (void)addObserverForKeyPath:(NSString *)keyPath
                      options:(NSKeyValueObservingOptions)options
                      context:(void *)context
                   usingBlock:(KVOBlock)block;

@end
