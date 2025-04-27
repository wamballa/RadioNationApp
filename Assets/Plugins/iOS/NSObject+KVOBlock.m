#import "NSObject+KVOBlock.h"
#import <objc/runtime.h>

@implementation NSObject (KVOBlock)

static const void *KVOBlockKey = &KVOBlockKey;

- (void)addObserverForKeyPath:(NSString *)keyPath
                      options:(NSKeyValueObservingOptions)options
                      context:(void *)context
                   usingBlock:(KVOBlock)block
{
    objc_setAssociatedObject(self, KVOBlockKey, block, OBJC_ASSOCIATION_COPY_NONATOMIC);
    [self addObserver:self forKeyPath:keyPath options:options context:context];
}

- (void)observeValueForKeyPath:(NSString *)keyPath
                      ofObject:(id)object
                        change:(NSDictionary<NSKeyValueChangeKey,id> *)change
                       context:(void *)context
{
    KVOBlock block = objc_getAssociatedObject(self, KVOBlockKey);
    if (block) {
        block(object, change);
    }
}

@end
